using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX002: Avoid throwing base exceptions
    /// Avoids throwing base exceptions like System.Exception or System.SystemException.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidBaseExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX002";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Avoid throwing base exceptions",
            "Throwing '{0}' is discouraged. Use a more specific exception type.",
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

            if (context.SemanticModel.GetSymbolInfo(objCreation.Type).Symbol is not INamedTypeSymbol type)
                return;

            var fullName = type.ToDisplayString();
            if (fullName == "System.Exception" || fullName == "System.SystemException")
            {
                var diagnostic = Diagnostic.Create(Rule, objCreation.GetLocation(), type.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
