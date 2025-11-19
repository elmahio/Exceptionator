using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class UnhandledDocumentedExceptionAnalyzerTests : AnalyzerTestBase<UnhandledDocumentedExceptionAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenDocumentedExceptionIsNotHandled()
        {
            var source = @"
using System;
class C
{
    public void Method1()
    {
        Method2();
    }

    /// <summary>
    /// <exception cref=""System.NullReferenceException""/>
    /// </summary>
    public void Method2()
    {
        var i = 0;
        var result = 21 / i;
        Console.WriteLine(result);
    }
}";

            var expected = new DiagnosticResult(UnhandledDocumentedExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("Call to 'Method2' may throw 'System.NullReferenceException' as documented, but the exception is not handled here.")
                .WithSpan(7, 9, 7, 18)
                .WithArguments("Method2", "System.NullReferenceException");

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenCallIsWrappedInSpecificCatch() =>
            await VerifyAnalyzerAsync(@"
using System;
class C
{
    public void Method1()
    {
        try
        {
            Method2();
        }
        catch (NullReferenceException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// <exception cref=""System.NullReferenceException""/>
    /// </summary>
    public void Method2()
    {
        var i = 0;
        var result = 21 / i;
        Console.WriteLine(result);
    }
}");

        [Test]
        public async Task DoesNotReportWhenCallIsWrappedInBaseExceptionCatch() =>
            await VerifyAnalyzerAsync(@"
using System;
class C
{
    public static void Method1()
    {
        try
        {
            Method2();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    /// <summary>
    /// <exception cref=""System.NullReferenceException""/>
    /// </summary>
    public static void Method2()
    {
        var i = 0;
        var result = 21 / i;
        Console.WriteLine(result);
    }
}");
    }
}
