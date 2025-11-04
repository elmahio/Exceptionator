using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExceptionAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MissingConstructorsCodeFixProvider)), Shared]
    public class MissingConstructorsCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create("EX021");

        public sealed override FixAllProvider GetFixAllProvider() =>
            WellKnownFixAllProviders.BatchFixer;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var diagnostic = context.Diagnostics.First();
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Add expected constructors",
                    ct => AddConstructorsAsync(context.Document, diagnostic, ct),
                    nameof(MissingConstructorsCodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> AddConstructorsAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (root == null || semanticModel == null)
                return document;

            if (root.FindNode(diagnostic.Location.SourceSpan) is not ClassDeclarationSyntax classDeclaration)
                return document;

            await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
            var constructors = classDeclaration.Members.OfType<ConstructorDeclarationSyntax>().ToList();

            bool hasMessageCtor = constructors.Any(c =>
                c.ParameterList.Parameters.Count == 1 &&
                c.ParameterList.Parameters[0].Type is PredefinedTypeSyntax pts &&
                pts.Keyword.IsKind(SyntaxKind.StringKeyword));

            bool hasMessageInnerCtor = constructors.Any(c =>
                c.ParameterList.Parameters.Count == 2 &&
                c.ParameterList.Parameters[0].Type is PredefinedTypeSyntax pts1 &&
                pts1.Keyword.IsKind(SyntaxKind.StringKeyword) &&
                c.ParameterList.Parameters[1].Type is IdentifierNameSyntax ins &&
                ins.Identifier.Text == "Exception");

            var newConstructors = new List<MemberDeclarationSyntax>();

            if (!hasMessageCtor)
            {
                newConstructors.Add(SyntaxFactory.ConstructorDeclaration(classDeclaration.Identifier.Text)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("message"))
                                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))))))
                    .WithInitializer(SyntaxFactory.ConstructorInitializer(
                        SyntaxKind.BaseConstructorInitializer,
                        SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("message"))))))
                    .WithBody(SyntaxFactory.Block()));
            }

            if (!hasMessageInnerCtor)
            {
                newConstructors.Add(SyntaxFactory.ConstructorDeclaration(classDeclaration.Identifier.Text)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithParameterList(SyntaxFactory.ParameterList(
                        SyntaxFactory.SeparatedList(
                        [
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("message"))
                                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))),
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("innerException"))
                                .WithType(SyntaxFactory.IdentifierName("Exception"))
                        ])))
                    .WithInitializer(SyntaxFactory.ConstructorInitializer(
                        SyntaxKind.BaseConstructorInitializer,
                        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                        [
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("message")),
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("innerException"))
                        ]))))
                    .WithBody(SyntaxFactory.Block()));
            }

            var newClass = classDeclaration.AddMembers([.. newConstructors]);
            var newRoot = root.ReplaceNode(classDeclaration, newClass);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
