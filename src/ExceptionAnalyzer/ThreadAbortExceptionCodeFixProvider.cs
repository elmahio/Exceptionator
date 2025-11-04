using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;

namespace ExceptionAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThreadAbortExceptionCodeFixProvider)), Shared]
    public class ThreadAbortExceptionCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add 'throw;' to preserve ThreadAbortException behavior";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(ThreadAbortExceptionAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty", Justification = "<Pending>")]
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root?.FindNode(context.Span) is not CatchClauseSyntax node)
                return;

            context.RegisterCodeFix(
                Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => AddRethrowAsync(context.Document, node, c),
                    equivalenceKey: Title),
                context.Diagnostics);
        }

        private static async Task<Document> AddRethrowAsync(Document document, CatchClauseSyntax catchClause, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            var throwStmt = SyntaxFactory.ThrowStatement();
            var updatedBlock = catchClause.Block.AddStatements(throwStmt);
            var updatedCatch = catchClause.WithBlock(updatedBlock);

            editor.ReplaceNode(catchClause, updatedCatch);
            return editor.GetChangedDocument();
        }
    }
}
