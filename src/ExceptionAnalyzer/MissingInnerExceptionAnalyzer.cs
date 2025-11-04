using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX003: Missing inner exception
    /// Ensures that newly thrown exceptions inside catch blocks include the original exception as inner exception.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingInnerExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX003";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Missing inner exception",
            "New exception should include the caught exception as inner exception.",
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
            if (throwStmt.Expression is not ObjectCreationExpressionSyntax objCreation)
                return;

            // Are we in a catch-block with an exception-symbol?
            var catchClause = throwStmt.FirstAncestorOrSelf<CatchClauseSyntax>();
            if (catchClause?.Declaration?.Identifier.ValueText is not string caughtVar)
                return;

            // If inner exception is already used as an argument, everything is ok
            if (objCreation.ArgumentList?.Arguments.Any(arg =>
                arg.Expression is IdentifierNameSyntax id && id.Identifier.ValueText == caughtVar) == true)
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, objCreation.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
