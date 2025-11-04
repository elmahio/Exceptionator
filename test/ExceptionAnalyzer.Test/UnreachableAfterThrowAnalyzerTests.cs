using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class UnreachableAfterThrowAnalyzerTests : AnalyzerTestBase<UnreachableAfterThrowAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticForStatementAfterThrow()
        {
            var source = @"
using System;
class C
{
    void M()
    {
        throw new Exception();
        Console.WriteLine(""x"");
    }
}";

            var expected = new DiagnosticResult(UnreachableAfterThrowAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("This code is unreachable because it's placed after a throw statement.")
                .WithSpan(8, 9, 8, 32);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenNoCodeAfterThrow() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    void M()
    {
        throw new Exception();
    }
}");
    }
}
