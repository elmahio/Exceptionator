using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace ExceptionAnalyzer
{
    /// <summary>
    /// EX001: Exception should include a message
    /// Ensures that exceptions are instantiated with a meaningful message.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ExceptionMustHaveMessageAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX001";

        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Exception should include a message",
            "Exception '{0}' should include a message.",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        }

        private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
        {
            var creation = (ObjectCreationExpressionSyntax)context.Node;

            if (context.SemanticModel.GetSymbolInfo(creation.Type).Symbol is not INamedTypeSymbol symbol || !InheritsFromOrEquals(symbol, "System.Exception"))
                return;

            var args = creation.ArgumentList?.Arguments;
            if (args == null || args.Value.Count == 0)
            {
                ReportDiagnostic();
                return;
            }

            if (context.SemanticModel.GetSymbolInfo(creation).Symbol is not IMethodSymbol ctorSymbol)
                return;

            // If the first parameter is a string
            if (ctorSymbol.Parameters.Length > 0 && ctorSymbol.Parameters[0].Type.SpecialType == SpecialType.System_String)
            {
                var firstArg = args.Value[0];
                var constant = context.SemanticModel.GetConstantValue(firstArg.Expression);

                if (constant.HasValue)
                {
                    // A string literal
                    if (constant.Value is string s)
                    {
                        if (string.IsNullOrWhiteSpace(s))
                        {
                            ReportDiagnostic();
                        }
                    }
                    // A null literal
                    else if (constant.Value == null)
                    {
                        ReportDiagnostic();
                    }
                    // Ignore everything else like numbers
                }
            }

            void ReportDiagnostic()
            {
                var diagnostic = Diagnostic.Create(Rule, creation.GetLocation(), symbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool InheritsFromOrEquals(INamedTypeSymbol? symbol, string baseType)
        {
            while (symbol != null)
            {
                if (symbol.ToDisplayString() == baseType)
                    return true;
                symbol = symbol.BaseType;
            }
            return false;
        }
    }
}