using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class PointlessTryCatchAnalyzerTests : AnalyzerTestBase<PointlessTryCatchAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenCatchOnlyThrows()
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
        catch (Exception)
        {
            throw;
        }
    }
}";

            var expected = new DiagnosticResult(PointlessTryCatchAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("This catch block only rethrows the exception. Consider removing it or adding meaningful handling.")
                .WithSpan(12, 13, 12, 19);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenCatchHasHandling()
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
            await VerifyAnalyzerAsync(source);
        }

        [Test]
        public async Task DoesNotReportWhenWrapping()
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
            throw new InvalidOperationException(""An error occurred."", ex);
        }
    }
}";
            await VerifyAnalyzerAsync(source);
        }
    }
}
