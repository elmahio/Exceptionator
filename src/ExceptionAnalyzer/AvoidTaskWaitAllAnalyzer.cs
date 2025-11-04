using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX010: Task.WaitAll should be wrapped with AggregateException catch
    /// Ensures proper exception handling for Task.WaitAll by requiring AggregateException catch or using Task.WhenAll.
    /// <para>See also:</para>
    /// https://www.code4it.dev/csharptips/task-whenall-vs-task-waitall-error-handling/
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AvoidTaskWaitAllAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX010";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Task.WaitAll should be wrapped with AggregateException catch",
            "Task.WaitAll should be used with caution – catch AggregateException or use await Task.WhenAll for proper exception handling.",
            "ExceptionHandling",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol || symbol.Name != "WaitAll")
                return;

            if (symbol.ContainingType.ToDisplayString() != "System.Threading.Tasks.Task")
                return;

            var tryStatement = invocation.FirstAncestorOrSelf<TryStatementSyntax>();
            if (tryStatement == null || tryStatement.Catches.Count == 0)
                return;

            var catchesAggregate = tryStatement.Catches.Any(c =>
            {
                // catch { } – catches all exceptions including AggregateException
                if (c.Declaration == null)
                    return true;

                var type = context.SemanticModel.GetTypeInfo(c.Declaration.Type).Type;
                return type?.ToDisplayString() == "System.AggregateException";
            });

            if (!catchesAggregate)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
            }
        }
    }
}
