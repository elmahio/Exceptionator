using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX005: Exception variable is unused
    /// Detects catch variables that are never used.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnusedExceptionVariableAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX005";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Exception variable is unused",
            "The caught exception '{0}' is never used.",
            "Usage",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeCatch, SyntaxKind.CatchClause);
        }

        private static void AnalyzeCatch(SyntaxNodeAnalysisContext context)
        {
            var catchClause = (CatchClauseSyntax)context.Node;
            var declaration = catchClause.Declaration;
            if (declaration == null)
                return;

            var identifier = declaration.Identifier;
            var name = identifier.ValueText;

            if (string.IsNullOrWhiteSpace(name))
                return;

            var block = catchClause.Block;
            var usedInBlock = block != null
                && block
                    .DescendantTokens()
                    .Any(t => t.IsKind(SyntaxKind.IdentifierToken) && t.ValueText == name);

            var filter = catchClause.Filter;
            var usedInFilter = filter != null
                && filter
                    .FilterExpression
                    .DescendantTokens()
                    .Any(t => t.IsKind(SyntaxKind.IdentifierToken) && t.ValueText == name);

            if (!usedInBlock && !usedInFilter)
            {
                var diagnostic = Diagnostic.Create(Rule, identifier.GetLocation(), name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
