using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX020: Exception class should be public
    /// Ensures that exception types are declared <code>public</code> to be visible when thrown or caught across assemblies.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExceptionShouldBePublicAnalyzer : CustomExceptionAnalyzerBase
    {
        public const string DiagnosticId = "EX020";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Exception class should be public",
            "Exception class '{0}' should be declared public.",
            "Design",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var declaration = (ClassDeclarationSyntax)context.Node;

            if (context.SemanticModel.GetDeclaredSymbol(declaration) is not INamedTypeSymbol symbol)
                return;

            if (!InheritsFromException(symbol))
                return;

            if (symbol.DeclaredAccessibility != Accessibility.Public)
            {
                var diagnostic = Diagnostic.Create(Rule, declaration.Identifier.GetLocation(), symbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
