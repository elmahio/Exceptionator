using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX024: Avoid catching fatal exceptions like StackOverflowException or ExecutionEngineException
    /// Flags catch blocks that handle fatal exceptions which should not be caught or are uncatchable.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidCatchingFatalExceptionsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX024";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Avoid catching fatal exceptions",
            "Catching '{0}' is discouraged or has no effect. These exceptions should not be caught.",
            "Correctness",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        private static readonly ImmutableHashSet<string> FatalExceptions = ImmutableHashSet.Create(
            "System.StackOverflowException",
            "System.ExecutionEngineException",
            "System.AccessViolationException",
            "System.OutOfMemoryException"
        );

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
            var typeSyntax = catchClause.Declaration?.Type;

            if (typeSyntax == null)
                return;

            var typeSymbol = context.SemanticModel.GetTypeInfo(typeSyntax).Type;
            if (typeSymbol == null)
                return;

            var typeName = typeSymbol.ToDisplayString();

            if (FatalExceptions.Contains(typeName))
            {
                var diagnostic = Diagnostic.Create(Rule, typeSyntax.GetLocation(), typeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
