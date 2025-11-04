using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX013: Avoid throwing ex.InnerException
    /// Detects when ex.InnerException is thrown directly, which may cause null reference issues and loses the stack trace.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThrowInnerExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX013";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Avoid throwing ex.InnerException",
            "Throwing ex.InnerException directly loses the original stack trace and may cause null reference exceptions.",
            "ExceptionHandling",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
        }

        private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
        {
            var throwStatement = (ThrowStatementSyntax)context.Node;
            if (throwStatement.Expression is MemberAccessExpressionSyntax memberAccess
                && memberAccess.Name.Identifier.Text == "InnerException"
                && memberAccess.Expression is IdentifierNameSyntax exIdentifier)
            {
                // Try to verify that exIdentifier refers to a caught exception
                var enclosingCatch = throwStatement.FirstAncestorOrSelf<CatchClauseSyntax>();
                if (enclosingCatch?.Declaration?.Identifier.Text == exIdentifier.Identifier.Text)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, throwStatement.GetLocation()));
                }
            }
        }
    }
}
