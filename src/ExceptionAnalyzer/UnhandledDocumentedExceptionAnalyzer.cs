using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Xml.Linq;
using System.Linq;

namespace ExceptionAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UnhandledDocumentedExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX025";

        private const string Category = "Usage";

        private static readonly LocalizableString Title =
            "Handle documented exceptions";

        private static readonly LocalizableString MessageFormat =
            "Call to '{0}' may throw '{1}' as documented, but the exception is not handled here.";

        private static readonly LocalizableString Description =
            "The called method documents that it may throw an exception via an <exception> XML comment. " +
            "The caller should either handle the documented exception or document that it may be thrown.";

        public static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            var semanticModel = context.SemanticModel;
            var cancellationToken = context.CancellationToken;

            var symbolInfo = semanticModel.GetSymbolInfo(invocation, cancellationToken);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
                return;

            // Get XML documentation and see if any <exception> tags are present
            var xml = methodSymbol.GetDocumentationCommentXml(cancellationToken: cancellationToken);
            if (string.IsNullOrWhiteSpace(xml))
                return;

            List<INamedTypeSymbol> documentedExceptionTypes;
            try
            {
                documentedExceptionTypes = GetDocumentedExceptionTypes(xml, semanticModel.Compilation);
            }
            catch
            {
                // If XML is malformed for any reason, just bail out quietly
                return;
            }

            if (documentedExceptionTypes.Count == 0) return;

            // Find nearest enclosing try statement (if any)
            var tryStatement = invocation.FirstAncestorOrSelf<TryStatementSyntax>();

            // If there is no try-catch around the call at all, we can directly report
            if (tryStatement is null)
            {
                var firstException = documentedExceptionTypes[0];
                ReportDiagnostic(context, invocation, methodSymbol, firstException);
                return;
            }

            // If there is a try/catch, check if *all* documented exceptions are handled.
            // We report for the first unhandled one.
            var firstUnhandledException = documentedExceptionTypes.FirstOrDefault(documentedException => !IsExceptionHandledByTry(tryStatement, documentedException, semanticModel, cancellationToken));
            if (firstUnhandledException != null)
            {
                ReportDiagnostic(context, invocation, methodSymbol, firstUnhandledException);
            }
        }

        private static List<INamedTypeSymbol> GetDocumentedExceptionTypes(string xml, Compilation compilation)
        {
            var result = new List<INamedTypeSymbol>();

            var doc = XDocument.Parse(xml);

            foreach (var exceptionElement in doc.Descendants("exception"))
            {
                var cref = (string?)exceptionElement.Attribute("cref");
                if (string.IsNullOrWhiteSpace(cref)) continue;

                var metadataName = CleanCref(cref!);
                if (string.IsNullOrWhiteSpace(metadataName)) continue;

                var type = compilation.GetTypeByMetadataName(metadataName);
                if (type is INamedTypeSymbol namedType)
                {
                    result.Add(namedType);
                }
            }

            return result;
        }

        private static string? CleanCref(string cref)
        {
            // Typical patterns:
            // "T:System.NullReferenceException"
            // "!:System.NullReferenceException"
            // "System.NullReferenceException"
            var trimmed = cref.Trim();

            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex >= 0)
            {
                trimmed = trimmed.Substring(colonIndex + 1);
            }

            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private static bool IsExceptionHandledByTry(
            TryStatementSyntax tryStatement,
            INamedTypeSymbol documentedException,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            foreach (var declaration in tryStatement.Catches.Select(c => c.Declaration))
            {
                // catch { } – catch all
                if (declaration is null) return true;

                if (semanticModel.GetTypeInfo(declaration.Type, cancellationToken).Type is not INamedTypeSymbol caughtType) continue;

                if (IsCaughtBy(documentedException, caughtType)) return true;
            }

            return false;
        }

        private static bool IsCaughtBy(INamedTypeSymbol documentedException, INamedTypeSymbol caughtType)
        {
            // Returns true if a catch(caughtType) would catch documentedException:
            // i.e. caughtType is the same as, or a base type of, documentedException.
            for (var current = documentedException; current is not null; current = current.BaseType)
            {
                if (SymbolEqualityComparer.Default.Equals(current, caughtType)) return true;
            }

            return false;
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            InvocationExpressionSyntax invocation,
            IMethodSymbol methodSymbol,
            INamedTypeSymbol exceptionType)
        {
            var properties = ImmutableDictionary<string, string>.Empty
                .Add("ExceptionType", exceptionType.ToDisplayString());

            var diagnostic = Diagnostic.Create(
                Rule,
                invocation.GetLocation(),
                properties,
                methodSymbol.Name,
                exceptionType.ToDisplayString());

            context.ReportDiagnostic(diagnostic);
        }
    }
}
