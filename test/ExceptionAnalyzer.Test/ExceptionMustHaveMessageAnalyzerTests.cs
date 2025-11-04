using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ExceptionMustHaveMessageAnalyzerTests : AnalyzerTestBase<ExceptionMustHaveMessageAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenExceptionHasNoMessage()
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

            const int startCol = 15;
            var endCol = startCol + creation.Length;

            var expected = new DiagnosticResult(ExceptionMustHaveMessageAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Exception 'Exception' should include a message.")
                .WithSpan(7, startCol, 7, endCol);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenExceptionHasMessage() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    void M()
    {
        throw new Exception(""boom"");
    }
}");
    }
}
