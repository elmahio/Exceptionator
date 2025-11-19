using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class UndocumentedExplicitExceptionCodeFixTests : CodeFixTestBase<UndocumentedExplicitExceptionAnalyzer, UndocumentedExplicitExceptionCodeFix>
    {
        [Test]
        public async Task AddsMissingExceptionDocumentation()
        {
            var before = @"
using System;
class C
{
    /// <summary>
    /// Does something.
    /// </summary>
    /// <exception cref=""System.InvalidOperationException""/>
    public void M()
    {
        if (DateTime.Now.Ticks == 0)
        {
            throw new InvalidOperationException();
        }

        throw new ArgumentException(""arg"");
    }
}";

            var after = @"
using System;
class C
{
    /// <summary>
    /// Does something.
    /// </summary>
    /// <exception cref=""System.InvalidOperationException""/>
    /// <exception cref=""System.ArgumentException""/>
    public void M()
    {
        if (DateTime.Now.Ticks == 0)
        {
            throw new InvalidOperationException();
        }

        throw new ArgumentException(""arg"");
    }
}";

            var expected = new DiagnosticResult(UndocumentedExplicitExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("Method 'M' throws 'System.ArgumentException' but does not document this exception, while other exceptions are documented.")
                .WithSpan(9, 17, 9, 18)   // <-- add this
                .WithArguments("M", "System.ArgumentException");

            await VerifyCodeFixAsync(before, after, expected);
        }
    }
}
