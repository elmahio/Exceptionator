using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX021: Missing expected constructors on custom exception
    /// Ensures that custom exceptions implement the expected constructors with message and inner exception parameters.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingConstructorsAnalyzer : CustomExceptionAnalyzerBase
    {
        public const string DiagnosticId = "EX021";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Exception must implement standard constructors",
            "Exception '{0}' must define constructors with (string message) and (string message, Exception innerException).",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;

            if (context.SemanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol)
                return;

            if (!InheritsFromException(classSymbol))
                return;

            var constructors = classSymbol.Constructors;

            bool hasMessageCtor = constructors.Any(c =>
                c.Parameters.Length == 1 &&
                c.Parameters[0].Type.SpecialType == SpecialType.System_String);

            bool hasMessageInnerCtor = constructors.Any(c =>
                c.Parameters.Length == 2 &&
                c.Parameters[0].Type.SpecialType == SpecialType.System_String &&
                c.Parameters[1].Type.ToDisplayString() == "System.Exception");

            if (!hasMessageCtor || !hasMessageInnerCtor)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, classDecl.Identifier.GetLocation(), classSymbol.Name));
            }
        }
    }
}
