using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace ExceptionAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AvoidThrowExCodeFixProvider)), Shared]
    public class AvoidThrowExCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Use 'throw;'";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(AvoidThrowExAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics[0];
            if (root?.FindNode(diagnostic.Location.SourceSpan) is not ThrowStatementSyntax throwStmt)
                return;

            context.RegisterCodeFix(
                Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => ReplaceWithThrowOnlyAsync(context.Document, throwStmt, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> ReplaceWithThrowOnlyAsync(Document document, ThrowStatementSyntax throwStmt, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);

            var newThrow = SyntaxFactory.ThrowStatement().WithTriviaFrom(throwStmt);

            editor.ReplaceNode(throwStmt, newThrow);
            return editor.GetChangedDocument();
        }
    }
}
