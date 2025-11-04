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
                .WithMessage("This try/catch block doesn't add any handling or logic.")
                .WithSpan(10, 9, 13, 10);

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
    }
}
