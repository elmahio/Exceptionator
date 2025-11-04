using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX004: Use 'throw;' instead of 'throw ex;'
    /// Preserves the original stack trace when rethrowing exceptions.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidThrowExAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX004";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Use 'throw;' instead of 'throw ex;'",
            "Use 'throw;' to preserve stack trace.",
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
            if (throwStmt.Expression is not IdentifierNameSyntax ident)
                return;

            // We are only looking for "throw ex;"
            var catchClause = throwStmt.FirstAncestorOrSelf<CatchClauseSyntax>();
            if (catchClause?.Declaration?.Identifier.ValueText != ident.Identifier.ValueText)
                return;

            var diagnostic = Diagnostic.Create(Rule, throwStmt.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
