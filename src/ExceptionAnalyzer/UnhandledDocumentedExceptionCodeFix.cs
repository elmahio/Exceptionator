using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExceptionAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnhandledDocumentedExceptionCodeFix))]
    [Shared]
    public sealed class UnhandledDocumentedExceptionCodeFix : CodeFixProvider
    {
        private const string Title = "Wrap call in try/catch for documented exception";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(UnhandledDocumentedExceptionAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var diagnostic = context.Diagnostics.First();
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    ct => WrapInTryCatchAsync(context.Document, diagnostic, ct),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> WrapInTryCatchAsync(
            Document document,
            Diagnostic diagnostic,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null) return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var invocation = node as InvocationExpressionSyntax
                             ?? node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            if (invocation is null) return document;

            // Get the containing statement (ExpressionStatement, LocalDeclarationStatement, etc.)
            var statement = invocation.FirstAncestorOrSelf<StatementSyntax>();
            if (statement is null) return document;

            // Get the documented exception type from the diagnostic properties
            // (set in the analyzer via Diagnostic.Properties["ExceptionType"])
            diagnostic.Properties.TryGetValue("ExceptionType", out var exceptionTypeName);
            if (string.IsNullOrWhiteSpace(exceptionTypeName))
            {
                exceptionTypeName = "System.Exception";
            }

            var catchDeclaration = SyntaxFactory.CatchDeclaration(SyntaxFactory.ParseTypeName(exceptionTypeName), SyntaxFactory.Identifier("ex"));

            var catchClause = SyntaxFactory.CatchClause(declaration: catchDeclaration, filter: null, block: SyntaxFactory.Block(SyntaxFactory.ThrowStatement()));

            var tryStatement = SyntaxFactory.TryStatement(block: SyntaxFactory.Block(statement), catches: SyntaxFactory.SingletonList(catchClause), @finally: null)
                .WithAdditionalAnnotations(Formatter.Annotation);

            var newRoot = root.ReplaceNode(statement, tryStatement);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
