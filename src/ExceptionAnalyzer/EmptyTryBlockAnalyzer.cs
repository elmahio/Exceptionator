using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX009: Empty try block with catch
    /// Detects try blocks that are empty while having a catch.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EmptyTryBlockAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX009";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Empty try block with catch",
            "Try block is empty but includes a catch – consider removing or adding meaningful code.",
            "CodeQuality",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeTryBlock, SyntaxKind.TryStatement);
        }

        private static void AnalyzeTryBlock(SyntaxNodeAnalysisContext context)
        {
            var tryStatement = (TryStatementSyntax)context.Node;

            if (tryStatement.Block == null || tryStatement.Block.Statements.Count != 0)
                return;

            if (tryStatement.Catches.Count == 0)
                return;

            var catchesException = tryStatement.Catches.Any(c =>
            {
                if (c.Declaration == null)
                    return true;

                var type = context.SemanticModel.GetTypeInfo(c.Declaration.Type).Type;
                return type?.ToDisplayString() == "System.Exception";
            });

            if (catchesException)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, tryStatement.TryKeyword.GetLocation()));
            }
        }
    }
}
