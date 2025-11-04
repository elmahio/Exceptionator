using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ThrowInnerExceptionAnalyzerTests
        : AnalyzerTestBase<ThrowInnerExceptionAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenThrowingInnerException()
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
            throw ex.InnerException;
        }
    }
}";

            var expected = new DiagnosticResult(ThrowInnerExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Throwing ex.InnerException directly loses the original stack trace and may cause null reference exceptions.")
                .WithSpan(12, 13, 12, 37);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenThrowingEx() => await VerifyAnalyzerAsync(@"
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
            throw;
        }
    }
}");
    }
}
