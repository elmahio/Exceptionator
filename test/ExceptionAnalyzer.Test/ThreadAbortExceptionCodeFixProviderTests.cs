using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ThreadAbortExceptionCodeFixProviderTests : CodeFixTestBase<ThreadAbortExceptionAnalyzer, ThreadAbortExceptionCodeFixProvider>
    {
        [Test]
        public async Task AddsRethrowToEmptyThreadAbortCatch()
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
            var fixedSource = @"
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
}";

            var expected = new DiagnosticResult(ThreadAbortExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("ThreadAbortException should be rethrown or reset using Thread.ResetAbort().")
                .WithSpan(11, 9, 13, 10);

            await VerifyCodeFixAsync(source, fixedSource, expected);
        }

        [Test]
        public async Task DoesNothingWhenThrowIsAlreadyPresent()
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
            throw;
        }
    }
}";
            await VerifyCodeFixAsync(source, source);
        }
    }
}
