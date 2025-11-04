using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX017: Avoid when clauses that always evaluate to true
    /// Detects <code>when</code> filters on catch blocks that always return true and are thus redundant.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CatchWhenAlwaysTrueAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX017";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Catch 'when' clause always true",
            "The 'when' filter always evaluates to true and is redundant.",
            "CodeQuality",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeCatchClause, SyntaxKind.CatchClause);
        }

        private static void AnalyzeCatchClause(SyntaxNodeAnalysisContext context)
        {
            var catchClause = (CatchClauseSyntax)context.Node;
            var filter = catchClause.Filter;
            if (filter == null)
                return;

            var constant = context.SemanticModel.GetConstantValue(filter.FilterExpression);
            if (constant.HasValue && constant.Value is bool b && b)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, filter.GetLocation()));
            }
        }
    }
}
