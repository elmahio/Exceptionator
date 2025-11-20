using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX007: Pointless try/catch block
    /// Detects try/catch blocks that don't add meaningful handling logic.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PointlessTryCatchAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX007";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Pointless try/catch block",
            "This catch block only rethrows the exception. Consider removing it or adding meaningful handling.",
            "CodeQuality",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeTryCatch, SyntaxKind.TryStatement);
        }

        private static void AnalyzeTryCatch(SyntaxNodeAnalysisContext context)
        {
            var tryStmt = (TryStatementSyntax)context.Node;

            foreach (var statements in tryStmt.Catches.Select(c => c.Block).Where(b => b != null).Select(b => b.Statements))
            {
                // We only care about a single statement in the catch body
                if (statements.Count != 1)
                    continue;

                if (statements[0] is not ThrowStatementSyntax throwStatement)
                    continue;

                // Only flag a *bare* "throw;" (no expression)
                if (throwStatement.Expression is null)
                {
                    // You can choose to report on the catch or on the throw
                    var location = throwStatement.GetLocation();
                    context.ReportDiagnostic(Diagnostic.Create(Rule, location));
                }
            }
        }
    }
}
