using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class EmptyTryBlockAnalyzerTests : AnalyzerTestBase<EmptyTryBlockAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenTryIsEmptyAndCatchesException()
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
        }
    }
}";

            var expected = new DiagnosticResult(EmptyTryBlockAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Try block is empty but includes a catch – consider removing or adding meaningful code.")
                .WithSpan(7, 9, 7, 12);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenTryHasStatements() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    void M()
    {
        try
        {
            Console.WriteLine(""x"");
        }
        catch (Exception)
        {
        }
    }
}");
    }
}
