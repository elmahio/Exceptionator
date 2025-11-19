using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class UnhandledDocumentedExceptionCodeFixTests : CodeFixTestBase<UnhandledDocumentedExceptionAnalyzer, UnhandledDocumentedExceptionCodeFix>
    {
        [Test]
        public async Task WrapsCallInTryCatchForDocumentedException()
        {
            var before = @"
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

            var after = @"
using System;
class C
{
    public void Method1()
    {
        try
        {
            Method2();
        }
        catch (System.NullReferenceException ex)
        {
            throw;
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
}";

            var expected = new DiagnosticResult(UnhandledDocumentedExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("Call to 'Method2' may throw 'System.NullReferenceException' as documented, but the exception is not handled here.")
                .WithSpan(7, 9, 7, 18)
                .WithArguments("Method2", "System.NullReferenceException");

            await VerifyCodeFixAsync(before, after, expected);
        }
    }
}
