using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ThreadAbortExceptionAnalyzerTests : AnalyzerTestBase<ThreadAbortExceptionAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenThreadAbortExceptionIsSwallowed()
        {
            var source = @"
using System;
using System.Threading;
class C
{
    void M()
    {
        try
        {
        }
        catch (ThreadAbortException)
        {
        }
    }
}";

            var expected = new DiagnosticResult(ThreadAbortExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("ThreadAbortException should be rethrown or reset using Thread.ResetAbort().")
                .WithSpan(11, 9, 13, 10);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenRethrown() => await VerifyAnalyzerAsync(@"
using System;
using System.Threading;
class C
{
    void M()
    {
        try
        {
        }
        catch (ThreadAbortException)
        {
            throw;
        }
    }
}");

        [Test]
        public async Task DoesNotReportDiagnosticWhenResetAbortIsCalled() => await VerifyAnalyzerAsync(@"
using System;
using System.Threading;
class C
{
    void M()
    {
        try
        {
        }
        catch (ThreadAbortException)
        {
            Thread.ResetAbort();
        }
    }
}");
    }
}
