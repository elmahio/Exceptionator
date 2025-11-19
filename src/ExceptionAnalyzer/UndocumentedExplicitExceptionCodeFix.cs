using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExceptionAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UndocumentedExplicitExceptionCodeFix))]
    [Shared]
    public class UndocumentedExplicitExceptionCodeFix : CodeFixProvider
    {
        private const string Title = "Add missing <exception> documentation";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(UndocumentedExplicitExceptionAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var diagnostic = context.Diagnostics.First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    ct => AddExceptionDocumentationAsync(context.Document, diagnostic, ct),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> AddExceptionDocumentationAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root is null) return document;

            var node = root.FindNode(diagnostic.Location.SourceSpan);
            var methodDeclarationSyntax = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (methodDeclarationSyntax is null) return document;

            diagnostic.Properties.TryGetValue("ExceptionType", out var exceptionTypeName);
            if (string.IsNullOrWhiteSpace(exceptionTypeName)) return document;

            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            // Line where the method starts
            var lineSpan = methodDeclarationSyntax.GetLocation().GetLineSpan();
            var methodStartLineIndex = lineSpan.StartLinePosition.Line;

            // Walk upwards to find the XML doc block and the last <exception> line
            var lastDocLineIndex = -1;
            var lastExceptionLineIndex = -1;

            for (var i = methodStartLineIndex - 1; i >= 0; i--)
            {
                var line = text.Lines[i];
                var lineText = line.ToString();

                // Stop when we leave the doc comment block
                if (!lineText.TrimStart().StartsWith("///"))
                    break;

                lastDocLineIndex = i;

                if (lineText.Contains("<exception"))
                    lastExceptionLineIndex = i;
            }

            if (lastDocLineIndex == -1 || lastExceptionLineIndex == -1)
                return document; // no doc comment or no existing <exception> – shouldn't happen given the analyzer

            // Insert after the last <exception> line
            var insertAfterLineIndex = lastExceptionLineIndex;
            var insertAfterLine = text.Lines[insertAfterLineIndex];

            // Reuse the indentation from that line (everything before "///")
            var insertLineText = insertAfterLine.ToString();
            var docIndex = insertLineText.IndexOf("///", StringComparison.Ordinal);
            var indent = docIndex > 0 ? insertLineText.Substring(0, docIndex) : string.Empty;

            var insertionPosition = insertAfterLine.EndIncludingLineBreak;

            var newLine = indent + "/// <exception cref=\"" + exceptionTypeName + "\"/>" + Environment.NewLine;

            var newText = text.WithChanges(new TextChange(new TextSpan(insertionPosition, 0), newLine));

            return document.WithText(newText);
        }
    }
}
