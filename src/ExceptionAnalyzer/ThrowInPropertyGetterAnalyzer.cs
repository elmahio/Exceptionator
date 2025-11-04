using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX012: Don't throw exceptions from property getters
    /// Discourages throwing exceptions directly from property getters.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ThrowInPropertyGetterAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX012";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Don't throw exceptions from property getters",
            "Avoid throwing exceptions from property getters.",
            "ExceptionHandling",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        }

        private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
        {
            var property = (PropertyDeclarationSyntax)context.Node;

            // Only care about properties with get accessors or expression bodies
            var getter = property.AccessorList?.Accessors.FirstOrDefault(a => a.Kind() == SyntaxKind.GetAccessorDeclaration);

            if (getter != null && getter.Body != null)
            {
                // Look for throw statements in block-bodied getters
                var throws = getter.Body.DescendantNodes().OfType<ThrowStatementSyntax>();
                foreach (var throwStmt in throws)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, throwStmt.GetLocation()));
                }
            }
            else if (getter != null && getter.ExpressionBody != null)
            {
                // Look for throw expressions in expression-bodied getters
                if (getter.ExpressionBody.Expression is ThrowExpressionSyntax throwExpr)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, throwExpr.GetLocation()));
                }
            }
            else if (property.ExpressionBody != null && property.ExpressionBody.Expression is ThrowExpressionSyntax throwExpr)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, throwExpr.GetLocation()));
            }
        }
    }
}
