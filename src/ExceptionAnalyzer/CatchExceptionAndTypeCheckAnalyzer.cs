using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX025: Catching Exception and then checking the type inside the catch.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class CatchExceptionAndTypeCheckAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX025";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Type check inside broad catch",
            "This catch handles 'Exception' but checks for '{0}'. Consider adding a dedicated catch clause.",
            "Usage",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeCatch, SyntaxKind.CatchClause);
        }

        private static void AnalyzeCatch(SyntaxNodeAnalysisContext context)
        {
            var catchClause = (CatchClauseSyntax)context.Node;
            if (catchClause.Declaration is null)
                return;

            var decl = catchClause.Declaration;
            var typeSymbol = context.SemanticModel.GetTypeInfo(decl.Type, context.CancellationToken).Type;
            if (typeSymbol is null)
                return;

            // only for System.Exception
            var isException =
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.Exception"
                || (typeSymbol.Name == "Exception" && typeSymbol.ContainingNamespace?.ToDisplayString() == "System");
            if (!isException)
                return;

            var exId = decl.Identifier;
            if (exId.IsKind(SyntaxKind.None))
                return;

            var block = catchClause.Block;
            if (block is null)
                return;

            foreach (var ifStmt in block.Statements.OfType<IfStatementSyntax>())
            {
                // case 1: if (ex is SomeType)
                if (ifStmt.Condition is BinaryExpressionSyntax bin
                    && bin.IsKind(SyntaxKind.IsExpression)
                    && bin.Left is IdentifierNameSyntax leftId
                    && leftId.Identifier.ValueText == exId.ValueText
                    && bin.Right is TypeSyntax rightType)
                {
                    var checkedType = context.SemanticModel.GetTypeInfo(rightType, context.CancellationToken).Type;
                    if (checkedType is null)
                        continue;

                    var diag = Diagnostic.Create(
                        Rule,
                        ifStmt.IfKeyword.GetLocation(),
                        checkedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                    context.ReportDiagnostic(diag);
                    return;
                }

                // case 2: if (ex is SomeType st)  – pattern form
                if (ifStmt.Condition is IsPatternExpressionSyntax isPattern
                    && isPattern.Expression is IdentifierNameSyntax idName
                    && idName.Identifier.ValueText == exId.ValueText)
                {
                    ITypeSymbol? checkedType = null;

                    if (isPattern.Pattern is DeclarationPatternSyntax declPat
                        && declPat.Type is TypeSyntax declType)
                    {
                        checkedType = context.SemanticModel.GetTypeInfo(declType, context.CancellationToken).Type;
                    }
                    else if (isPattern.Pattern is ConstantPatternSyntax constPat
                        && constPat.Expression is TypeSyntax constType)
                    {
                        checkedType = context.SemanticModel.GetTypeInfo(constType, context.CancellationToken).Type;
                    }

                    if (checkedType is null)
                        continue;

                    var diag = Diagnostic.Create(
                        Rule,
                        ifStmt.IfKeyword.GetLocation(),
                        checkedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
                    context.ReportDiagnostic(diag);
                    return;
                }
            }
        }
    }
}
