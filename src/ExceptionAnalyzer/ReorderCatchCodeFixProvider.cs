using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExceptionAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReorderCatchCodeFixProvider))]
    [Shared]
    public sealed class ReorderCatchCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Reorder catch clauses";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create("CS0160");

        public override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root is null)
                return;

            var diagnostic = context.Diagnostics.First();
            var node = root.FindNode(diagnostic.Location.SourceSpan);

            var catchClause = node.FirstAncestorOrSelf<CatchClauseSyntax>();
            if (catchClause is null)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    ct => ReorderAsync(context.Document, root, catchClause),
                    Title),
                diagnostic);
        }

        private static Task<Document> ReorderAsync(Document document, SyntaxNode root, CatchClauseSyntax catchClause)
        {
            if (catchClause.Parent is not TryStatementSyntax tryStmt)
                return Task.FromResult(document);

            var catches = tryStmt.Catches;
            var index = catches.IndexOf(catchClause);
            if (index <= 0)
                return Task.FromResult(document);

            var newCatches = catches.RemoveAt(index);
            newCatches = newCatches.Insert(index - 1, catchClause);

            var newTry = tryStmt.WithCatches(newCatches);
            var newRoot = root.ReplaceNode(tryStmt, newTry);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
