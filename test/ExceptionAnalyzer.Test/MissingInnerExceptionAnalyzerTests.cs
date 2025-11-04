using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class MissingInnerExceptionAnalyzerTests : AnalyzerTestBase<MissingInnerExceptionAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenInnerExceptionIsMissing()
        {
            var source = @"
using System;
class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(""msg"");
        }
    }
}";

            var expected = new DiagnosticResult(MissingInnerExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("New exception should include the caught exception as inner exception.")
                .WithSpan(12, 19, 12, 55);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenInnerExceptionIsPassed() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(""msg"", ex);
        }
    }
}");
    }
}
