using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX019: Avoid general catch-all without any handling
    /// Detects general catch blocks that don't include logging, rethrow, or even a comment.
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
            context.RegisterSyntaxNodeAction(AnalyzeThrow, SyntaxKind.ThrowStatement);
        }

        private static void AnalyzeThrow(SyntaxNodeAnalysisContext context)
        {
            var throwStmt = (ThrowStatementSyntax)context.Node;
            if (throwStmt.Expression is not ObjectCreationExpressionSyntax creation)
                return;

            if (context.SemanticModel.GetSymbolInfo(creation.Type).Symbol is not INamedTypeSymbol symbol)
                return;

            if (symbol.ToDisplayString() == "System.NotImplementedException")
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, throwStmt.GetLocation()));
            }
        }
    }
}
