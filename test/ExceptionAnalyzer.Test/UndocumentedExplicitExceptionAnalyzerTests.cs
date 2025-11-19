using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class UndocumentedExplicitExceptionAnalyzerTests : AnalyzerTestBase<UndocumentedExplicitExceptionAnalyzer>
    {
        [Test]
        public async Task ReportsWhenThrownExceptionIsNotDocumented()
        {
            var source = @"
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

            var expected = new DiagnosticResult(UndocumentedExplicitExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("Method 'M' throws 'System.ArgumentException' but does not document this exception, while other exceptions are documented.")
                // Method identifier M on line 9, column 17
                .WithSpan(9, 17, 9, 18)
                .WithArguments("M", "System.ArgumentException");

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenAllThrownExceptionsAreDocumented() =>
            await VerifyAnalyzerAsync(@"
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
        throw new InvalidOperationException();
        throw new ArgumentException(""arg"");
    }
}");

        [Test]
        public async Task DoesNotReportWhenMethodHasNoExceptionDocs() =>
            await VerifyAnalyzerAsync(@"
using System;
class C
{
    /// <summary>
    /// Does something.
    /// </summary>
    public void M()
    {
        throw new ArgumentException(""arg"");
    }
}");

        [Test]
        public async Task DoesNotReportWhenNoThrowStatements() =>
            await VerifyAnalyzerAsync(@"
using System;
class C
{
    /// <summary>
    /// Does something.
    /// </summary>
    /// <exception cref=""System.ArgumentException""/>
    public void M()
    {
        Console.WriteLine(""No throws here"");
    }
}");
    }
}
