using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class LogOnlyExceptionMessageAnalyzerTests : AnalyzerTestBase<LogOnlyExceptionMessageAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenOnlyExMessageIsLogged()
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
            Console.WriteLine(ex.Message);
        }
    }
}";

            var expected = new DiagnosticResult(LogOnlyExceptionMessageAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Log the entire exception to preserve stack trace and context.")
                .WithSpan(12, 13, 12, 42);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenBothMessageAndExceptionAreLogged() => await VerifyAnalyzerAsync(@"
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
            Console.WriteLine(""Error: "" + ex.Message, ex);
        }
    }
}");
    }
}
