using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace ExceptionAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FilterExceptionManuallyCodeFixProvider))]
    [Shared]
    public sealed class FilterExceptionManuallyCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Introduce specific catch";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(FilterExceptionManuallyAnalyzer.DiagnosticId); // EX018

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

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
                    ct => ApplyAsync(context.Document, root, catchClause, node),
                    Title),
                diagnostic);
        }

        private static Task<Document> ApplyAsync(
    Document document,
    SyntaxNode root,
    CatchClauseSyntax catchClause,
    SyntaxNode diagnosticNode)
        {
            var ifStatement = diagnosticNode.FirstAncestorOrSelf<IfStatementSyntax>();
            if (ifStatement is null)
                return Task.FromResult(document);

            // 1) get specific type
            TypeSyntax? specificTypeSyntax = null;

            if (ifStatement.Condition is BinaryExpressionSyntax bin &&
                bin.IsKind(SyntaxKind.IsExpression) &&
                bin.Right is TypeSyntax binType)
            {
                specificTypeSyntax = binType;
            }
            else if (ifStatement.Condition is IsPatternExpressionSyntax isPattern &&
                     isPattern.Pattern is DeclarationPatternSyntax declPat)
            {
                specificTypeSyntax = declPat.Type;
            }

            if (specificTypeSyntax is null)
                return Task.FromResult(document);

            // 2) identifier to reuse
            var broadId = catchClause.Declaration?.Identifier ?? default;
            var exIdentifier = broadId.IsKind(SyntaxKind.None)
                ? SyntaxFactory.Identifier("ex")
                : broadId;

            // 3) lifted statements for the new specific catch (the 'if' body)
            SyntaxList<StatementSyntax> liftedStatements =
                ifStatement.Statement is BlockSyntax ifBlock
                    ? ifBlock.Statements
                    : SyntaxFactory.SingletonList(ifStatement.Statement);

            var specificCatch = SyntaxFactory
                .CatchClause()
                .WithDeclaration(SyntaxFactory.CatchDeclaration(specificTypeSyntax, exIdentifier))
                .WithBlock(SyntaxFactory.Block(liftedStatements));

            // 4) rebuild the original catch block
            //    - remove the if-statement
            //    - but if there is an else, keep its statements
            var originalBlock = catchClause.Block;
            var remaining = originalBlock.Statements.ToList();
            var indexOfIf = remaining.IndexOf(ifStatement);
            if (indexOfIf >= 0)
            {
                remaining.RemoveAt(indexOfIf);

                if (ifStatement.Else is not null)
                {
                    if (ifStatement.Else.Statement is BlockSyntax elseBlock)
                    {
                        remaining.InsertRange(indexOfIf, elseBlock.Statements);
                    }
                    else
                    {
                        remaining.Insert(indexOfIf, ifStatement.Else.Statement);
                    }
                }
            }

            var newBroadBlock = SyntaxFactory.Block(remaining);
            var newBroadCatch = catchClause.WithBlock(newBroadBlock);

            // 5) insert both catches back into the try
            if (catchClause.Parent is not TryStatementSyntax tryStmt)
                return Task.FromResult(document);

            var catches = tryStmt.Catches;
            var idx = catches.IndexOf(catchClause);

            var newCatches = catches.RemoveAt(idx);
            newCatches = newCatches.Insert(idx, newBroadCatch);
            newCatches = newCatches.Insert(idx, specificCatch);

            var newTry = tryStmt.WithCatches(newCatches);
            var newRoot = root.ReplaceNode(tryStmt, newTry);

            return Task.FromResult(document.WithSyntaxRoot(newRoot));
        }
    }
}
