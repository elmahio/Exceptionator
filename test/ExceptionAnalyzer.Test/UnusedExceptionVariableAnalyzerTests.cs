using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class UnusedExceptionVariableAnalyzerTests : AnalyzerTestBase<UnusedExceptionVariableAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenExceptionVariableIsUnused()
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
        }
    }
}";

            var expected = new DiagnosticResult(UnusedExceptionVariableAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("The caught exception 'ex' is never used.")
                .WithSpan(10, 26, 10, 28)
                .WithArguments("ex");

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenExceptionVariableIsUsed() => await VerifyAnalyzerAsync(@"
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
}");

        [Test]
        public async Task DoesNotReportWhenExceptionVariableIsUsedInFilter() => await VerifyAnalyzerAsync(@"
using System;
using System.Threading.Tasks;

class C
{
    async Task M()
    {
        try
        {
        }
        catch (Exception ex) when (ex is OperationCanceledException)
        {
            // ignored
        }
    }
}");
    }
}
