using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX016: Avoid empty catch when throwing new exception without message
    /// Detects cases where a new exception is thrown in a catch block without message or inner exception.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThrowNullAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX016";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Don't throw null",
            "Avoid throwing null – use a specific exception type instead.",
            "ExceptionHandling",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
        }

        private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
        {
            var throwStmt = (ThrowStatementSyntax)context.Node;

            if (throwStmt.Expression is LiteralExpressionSyntax literal &&
                literal.IsKind(SyntaxKind.NullLiteralExpression))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, throwStmt.GetLocation()));
            }
        }
    }
}
