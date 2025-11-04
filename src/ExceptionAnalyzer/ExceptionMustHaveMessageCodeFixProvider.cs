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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExceptionMustHaveMessageCodeFixProvider)), Shared]
    public class ExceptionMustHaveMessageCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Add TODO message";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(ExceptionMustHaveMessageAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics[0];
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            if (root?.FindNode(diagnosticSpan) is not ObjectCreationExpressionSyntax node) return;

            context.RegisterCodeFix(
                Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => AddTodoMessageAsync(context.Document, node, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> AddTodoMessageAsync(Document document, ObjectCreationExpressionSyntax creation, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
            var newArg = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("TODO: Add message")));

            var newArgs = creation.ArgumentList == null
                ? SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([newArg]))
                : creation.ArgumentList.WithArguments(SyntaxFactory.SeparatedList([newArg]));

            var newCreation = creation.WithArgumentList(newArgs);
            editor.ReplaceNode(creation, newCreation);

            return editor.GetChangedDocument();
        }
    }
}
