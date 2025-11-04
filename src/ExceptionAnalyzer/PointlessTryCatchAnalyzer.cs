using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            "This try/catch block doesn't add any handling or logic.",
            "CodeQuality",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeTryCatch, SyntaxKind.TryStatement);
        }

        private static void AnalyzeTryCatch(SyntaxNodeAnalysisContext context)
        {
            var tryStmt = (TryStatementSyntax)context.Node;

            // Example: try only containing a throw
            if (tryStmt.Block?.Statements.Count == 1 &&
                tryStmt.Block.Statements[0] is ThrowStatementSyntax &&
                tryStmt.Catches.Count == 0)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, tryStmt.GetLocation()));
                return;
            }

            foreach (var catchClause in tryStmt.Catches)
            {
                var block = catchClause.Block;
                if (block != null && block.Statements.Count == 1 && block.Statements[0] is ThrowStatementSyntax)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, catchClause.GetLocation()));
                }
            }
        }
    }
}
