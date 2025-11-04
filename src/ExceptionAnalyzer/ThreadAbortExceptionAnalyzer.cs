using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX008: ThreadAbortException must not be swallowed
    /// Ensures ThreadAbortException is either rethrown or reset.
    /// The problem in this analyzer is described here: https://andrewlock.net/creating-an-analyzer-to-detect-infinite-loops-caused-by-threadabortexception/
    /// The post uses a while loop as an example, but the same issue can occur in catch blocks for ThreadAbortException.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThreadAbortExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX008";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "ThreadAbortException must not be swallowed",
            "ThreadAbortException should be rethrown or reset using Thread.ResetAbort().",
            "Correctness",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var catchClause = (CatchClauseSyntax)context.Node;
            if (catchClause.Declaration == null || catchClause.Block == null)
                return;

            var type = context.SemanticModel.GetTypeInfo(catchClause.Declaration.Type).Type;
            if (type == null || type.ToDisplayString() != "System.Threading.ThreadAbortException")
                return;

            var block = catchClause.Block;

            // Check for throw
            var hasRethrow = block.Statements.OfType<ThrowStatementSyntax>().Any(ts => ts.Expression == null);

            // Check for Thread.ResetAbort() anywhere in block
            var hasReset = block.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(invocation =>
                {
                    var symbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    return symbol?.ToDisplayString() == "System.Threading.Thread.ResetAbort()";
                });

            if (!hasRethrow && !hasReset)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, catchClause.GetLocation()));
            }
        }
    }
}
