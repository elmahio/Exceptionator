using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ExceptionToStringLoggingAnalyzerTests : AnalyzerTestBase<ExceptionToStringLoggingAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenLoggingExceptionToStringInConcat()
        {
            var source = @"
using System;
class C
{
    void M(Exception ex)
    {
        Console.WriteLine(""Error: "" + ex.ToString());
    }
}";

            var expected = new DiagnosticResult(ExceptionToStringLoggingAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Log the exception object directly instead of using ex.ToString() to preserve stack trace and structure.")
                .WithSpan(7, 27, 7, 52);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenLoggingExceptionDirectly() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    void M(Exception ex)
    {
        Console.WriteLine(ex);
    }
}");
    }
}
