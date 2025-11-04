using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExceptionAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UnusedExceptionVariableCodeFixProvider)), Shared]
    public class UnusedExceptionVariableCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Remove unused exception variable";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(UnusedExceptionVariableAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics[0];

            var catchClause = root!
                .FindNode(diagnostic.Location.SourceSpan)
                .AncestorsAndSelf()
                .OfType<CatchClauseSyntax>()
                .FirstOrDefault();

            if (catchClause?.Declaration is null)
                return;

            context.RegisterCodeFix(
                Microsoft.CodeAnalysis.CodeActions.CodeAction.Create(
                    Title,
                    c => RemoveExceptionVariableAsync(context.Document, catchClause, c),
                    Title),
                diagnostic);
        }

        private static async Task<Document> RemoveExceptionVariableAsync(Document document, CatchClauseSyntax catchClause, CancellationToken cancellationToken)
        {
            var oldDeclaration = catchClause.Declaration!;
            var newDeclaration = SyntaxFactory
                .CatchDeclaration(oldDeclaration.Type.WithoutTrailingTrivia())
                .WithOpenParenToken(oldDeclaration.OpenParenToken)
                .WithCloseParenToken(oldDeclaration.CloseParenToken);

            var newCatch = catchClause.WithDeclaration(newDeclaration);

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root!.ReplaceNode(catchClause, newCatch);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
