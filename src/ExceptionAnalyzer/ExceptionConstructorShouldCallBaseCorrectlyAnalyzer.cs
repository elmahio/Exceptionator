using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX022: Exception constructors must call base
    /// Ensures that exception constructors pass their parameters (message, innerException) to the base constructor.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExceptionConstructorShouldCallBaseCorrectlyAnalyzer : CustomExceptionAnalyzerBase
    {
        public const string DiagnosticId = "EX022";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Exception constructor must call base constructor correctly",
            "Constructor should pass '{0}' to base constructor",
            "ExceptionHandling",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
        }

        private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
        {
            var ctor = (ConstructorDeclarationSyntax)context.Node;

            if (ctor.ParameterList == null || ctor.ParameterList.Parameters.Count == 0)
                return;

            var classDecl = ctor.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDecl == null)
                return;

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
            if (!InheritsFromException(classSymbol))
                return;

            var parameters = ctor.ParameterList.Parameters;
            if (parameters.Count == 1 &&
                parameters[0].Type is PredefinedTypeSyntax p1 &&
                p1.Keyword.IsKind(SyntaxKind.StringKeyword))
            {
                CheckBaseCall(context, ctor, ["message"]);
            }
            else if (parameters.Count == 2 &&
                     parameters[0].Type is PredefinedTypeSyntax p2 &&
                     p2.Keyword.IsKind(SyntaxKind.StringKeyword) &&
                     parameters[1].Type is IdentifierNameSyntax id2 &&
                     id2.Identifier.Text == "Exception")
            {
                CheckBaseCall(context, ctor, ["message", parameters[1].Identifier.Text]);
            }
        }

        private static void CheckBaseCall(SyntaxNodeAnalysisContext context, ConstructorDeclarationSyntax ctor, string[] expectedArgs)
        {
            if (ctor.Initializer == null || !ctor.Initializer.IsKind(SyntaxKind.BaseConstructorInitializer))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, ctor.Identifier.GetLocation(), string.Join(", ", expectedArgs)));
                return;
            }

            var actualArgs = ctor.Initializer.ArgumentList?.Arguments.Select(a => a.ToString()).ToArray() ?? [];
            foreach (var expected in expectedArgs.Where(expected => !actualArgs.Contains(expected)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, ctor.Initializer.GetLocation(), expected));
            }
        }
    }
}
