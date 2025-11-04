using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ExceptionAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingInnerExceptionCodeFixProvider)), Shared]
    public class MissingInnerExceptionCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Include inner exception";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(MissingInnerExceptionAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics[0];
            if (root?.FindNode(diagnostic.Location.SourceSpan) is not ObjectCreationExpressionSyntax node)
                return;

            context.RegisterCodeFix(
                Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => AddInnerExceptionAsync(context.Document, node, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> AddInnerExceptionAsync(Document document, ObjectCreationExpressionSyntax creation, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            // Find catch-blok og navnet på exceptionvariablen (fx ex)
            var catchClause = creation.FirstAncestorOrSelf<CatchClauseSyntax>();
            var caughtVar = catchClause?.Declaration?.Identifier.ValueText;
            if (string.IsNullOrEmpty(caughtVar))
                return document;

            var identifier = SyntaxFactory.IdentifierName(caughtVar);
            var newArg = SyntaxFactory.Argument(identifier);

            // Tilføj som sidste argument
            var updatedArgs = creation.ArgumentList != null
                ? creation.ArgumentList.Arguments.Add(newArg)
                : SyntaxFactory.SeparatedList([newArg]);

            var newArgList = creation.ArgumentList != null
                ? creation.ArgumentList.WithArguments(updatedArgs)
                : SyntaxFactory.ArgumentList(updatedArgs);

            var updatedCreation = creation.WithArgumentList(newArgList);
            editor.ReplaceNode(creation, updatedCreation);

            return editor.GetChangedDocument();
        }
    }
}
