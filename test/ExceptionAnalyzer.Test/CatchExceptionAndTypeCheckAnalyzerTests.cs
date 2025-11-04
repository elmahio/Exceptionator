using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class CatchExceptionAndTypeCheckAnalyzerTests : AnalyzerTestBase<CatchExceptionAndTypeCheckAnalyzer>
    {
        [Test]
        public async Task ReportsWhenCatchingExceptionAndCheckingType()
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
                Console.WriteLine(""invalid"");
            }
        }
    }
}";

            var expected = new DiagnosticResult(CatchExceptionAndTypeCheckAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("This catch handles 'Exception' but checks for 'InvalidOperationException'. Consider adding a dedicated catch clause.")
                .WithSpan(12, 13, 12, 15);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenNoTypeCheck()
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
            Console.WriteLine(ex);
        }
    }
}";
            await VerifyAnalyzerAsync(source);
        }

        [Test]
        public async Task DoesNotReportWhenCatchingSpecificException()
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
        catch (InvalidOperationException ex)
        {
            if (ex is InvalidOperationException)
            {
                Console.WriteLine(""ok"");
            }
        }
    }
}";
            await VerifyAnalyzerAsync(source);
        }
    }
}
