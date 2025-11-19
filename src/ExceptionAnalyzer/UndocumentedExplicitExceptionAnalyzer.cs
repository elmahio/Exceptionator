using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace ExceptionAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UndocumentedExplicitExceptionAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "EX026";

        private const string Category = "Documentation";

        private static readonly LocalizableString Title = "Document explicitly thrown exceptions";

        private static readonly LocalizableString MessageFormat = "Method '{0}' throws '{1}' but does not document this exception, while other exceptions are documented.";

        private static readonly LocalizableString Description =
            "The method already documents one or more exceptions using <exception> XML tags, " +
            "but omits other exceptions that are explicitly thrown using 'throw' in the method body.";

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

            context.RegisterSyntaxNodeAction(AnalyzeMethod, SyntaxKind.MethodDeclaration);
        }

        private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            // Only consider methods that actually have XML documentation trivia.
            var trivia = methodDeclaration
                .GetLeadingTrivia()
                .FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia));

            if (trivia.Equals(default)) return;

            var semanticModel = context.SemanticModel;
            var compilation = semanticModel.Compilation;
            var cancellationToken = context.CancellationToken;

            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);
            if (methodSymbol is null) return;

            var xml = methodSymbol.GetDocumentationCommentXml(cancellationToken: cancellationToken);
            if (string.IsNullOrWhiteSpace(xml)) return;

            List<INamedTypeSymbol> documentedExceptionTypes;
            try
            {
                documentedExceptionTypes = GetDocumentedExceptionTypes(xml, compilation);
            }
            catch
            {
                // Malformed XML or similar – ignore.
                return;
            }

            // Rule only applies when there is at least one <exception> tag already.
            if (documentedExceptionTypes.Count == 0) return;

            var thrownExceptionTypes = GetExplicitlyThrownExceptionTypes(methodDeclaration, semanticModel, cancellationToken);

            if (thrownExceptionTypes.Count == 0) return;

            var documentedSet = new HashSet<INamedTypeSymbol>(documentedExceptionTypes, SymbolEqualityComparer.Default);

            var missingTypes = thrownExceptionTypes
                .OfType<INamedTypeSymbol>() // ensure only named types
                .Where(t => !documentedSet.Contains(t))
                .Distinct(SymbolEqualityComparer.Default)
                .ToList();

            if (missingTypes.Count == 0)
                return;

            // Report one diagnostic per missing type. Tests so far use a single missing type.
            foreach (var missingType in missingTypes)
            {
                if (missingType is INamedTypeSymbol namedType)
                {
                    ReportDiagnostic(context, methodDeclaration, methodSymbol, namedType);
                }
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
            // "T:System.ArgumentException"
            // "!:System.ArgumentException"
            // "System.ArgumentException"
            var trimmed = cref.Trim();

            var colonIndex = trimmed.IndexOf(':');
            if (colonIndex >= 0)
            {
                trimmed = trimmed.Substring(colonIndex + 1);
            }

            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        private static List<INamedTypeSymbol> GetExplicitlyThrownExceptionTypes(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            var result = new List<INamedTypeSymbol>();

            // Collect throw statements
            var throwStatements = methodDeclaration
                .DescendantNodes()
                .OfType<ThrowStatementSyntax>();

            foreach (var expression in throwStatements.Where(ts => ts.Expression != null).Select(ts => ts.Expression))
            {
                var type = GetExceptionTypeFromExpression(expression, semanticModel, cancellationToken);
                if (type is not null)
                {
                    result.Add(type);
                }
            }

            // Collect throw expressions (e.g. in ?: or ??)
            var throwExpressions = methodDeclaration
                .DescendantNodes()
                .OfType<ThrowExpressionSyntax>();

            foreach (var throwExpression in throwExpressions.Where(tw => tw.Expression != null).Select(ts => ts.Expression))
            {
                var type = GetExceptionTypeFromExpression(throwExpression, semanticModel, cancellationToken);
                if (type is not null)
                {
                    result.Add(type);
                }
            }

            return result;
        }

        private static INamedTypeSymbol? GetExceptionTypeFromExpression(ExpressionSyntax expression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            // We focus on "throw new SomeException(...)" patterns.
            if (expression is ObjectCreationExpressionSyntax objectCreation)
            {
                var typeInfo = semanticModel.GetTypeInfo(objectCreation, cancellationToken).Type;
                return typeInfo as INamedTypeSymbol;
            }

            // Other patterns like "throw existingException;" are ignored for this rule.
            return null;
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration, IMethodSymbol methodSymbol, INamedTypeSymbol missingType)
        {
            var properties = ImmutableDictionary<string, string>.Empty
                .Add("ExceptionType", missingType.ToDisplayString());

            var diagnostic = Diagnostic.Create(
                Rule,
                methodDeclaration.Identifier.GetLocation(),
                properties,
                methodSymbol.Name,
                missingType.ToDisplayString());

            context.ReportDiagnostic(diagnostic);
        }
    }
}
