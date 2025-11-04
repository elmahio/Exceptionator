using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ExceptionMustHaveMessageCodeFixProviderTests : CodeFixTestBase<ExceptionMustHaveMessageAnalyzer, ExceptionMustHaveMessageCodeFixProvider>
    {
        [Test]
        public async Task AddsTodoMessageToParameterlessException()
        {
            var creation = "new Exception()";
            var source = $@"
using System;
class C
{{
    void M()
    {{
        throw {creation};
    }}
}}";

            var fixedSource = @"
using System;
class C
{
    void M()
    {
        throw new Exception(""TODO: Add message"");
    }
}";

            const int startCol = 15;
            var endCol = startCol + creation.Length;

            var expected = new DiagnosticResult(ExceptionMustHaveMessageAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Exception 'Exception' should include a message.")
                .WithSpan(7, startCol, 7, endCol);

            await VerifyCodeFixAsync(source, fixedSource, expected);
        }

        [Test]
        public async Task DoesNotReportWhenMessageAlreadyPresent()
        {
            var source = @"
using System;
class C
{
    void M()
    {
        throw new Exception(""already"");
    }
}";
            await VerifyCodeFixAsync(source, source);
        }
    }
}
