using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class FilterExceptionManuallyAnalyzerTests : AnalyzerTestBase<FilterExceptionManuallyAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenExceptionIsFilteredManuallyInsideCatch()
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
            if (ex is InvalidOperationException)
            {
            }
        }
    }
}";

            var expected = new DiagnosticResult(FilterExceptionManuallyAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("Use exception filters or catch specific exception types instead of 'if (ex is ...)'.")
                .WithSpan(12, 13, 14, 14);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenUsingExceptionFilter() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
        }
    }
}");
    }
}
