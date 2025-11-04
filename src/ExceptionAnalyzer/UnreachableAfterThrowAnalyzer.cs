using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX006: Unreachable code after throw
    /// Detects code written after a throw statement that will never execute.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnreachableAfterThrowAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX006";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Unreachable code after throw",
            "This code is unreachable because it's placed after a throw statement.",
            "CodeQuality",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
        }

        private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
        {
            var block = (BlockSyntax)context.Node;
            bool foundThrow = false;

            foreach (var statement in block.Statements)
            {
                if (foundThrow)
                {
                    // Everything after throw is unreachable
                    context.ReportDiagnostic(Diagnostic.Create(Rule, statement.GetLocation()));
                    continue;
                }

                if (statement is ThrowStatementSyntax)
                {
                    foundThrow = true;
                }
            }
        }
    }
}
