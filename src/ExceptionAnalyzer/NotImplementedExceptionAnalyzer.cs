using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX019: NotImplementedException left in code
    /// Detects <code>throw new NotImplementedException()</code> left in methods or properties.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NotImplementedExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX019";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "NotImplementedException left in code",
            "Avoid leaving 'throw new NotImplementedException()' in production code.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
            context.RegisterSyntaxNodeAction(AnalyzeThrowExpression, SyntaxKind.ThrowExpression);
        }

        private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
        {
            var throwStmt = (ThrowStatementSyntax)context.Node;
            AnalyzeCreation(context, throwStmt.Expression, throwStmt.GetLocation());
        }

        private static void AnalyzeThrowExpression(SyntaxNodeAnalysisContext context)
        {
            var throwExpr = (ThrowExpressionSyntax)context.Node;
            AnalyzeCreation(context, throwExpr.Expression, throwExpr.GetLocation());
        }

        private static void AnalyzeCreation(SyntaxNodeAnalysisContext context, ExpressionSyntax? expr, Location location)
        {
            if (expr is not ObjectCreationExpressionSyntax creation)
                return;

            if (context.SemanticModel.GetSymbolInfo(creation.Type).Symbol is not INamedTypeSymbol symbol)
                return;

            if (symbol.ToDisplayString() == "System.NotImplementedException")
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, location));
            }
        }
    }
}
