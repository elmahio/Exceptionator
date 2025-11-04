using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class EmptyCatchBlockAnalyzerTests : AnalyzerTestBase<EmptyCatchBlockAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenCatchBlockIsEmpty()
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

            var expected = new DiagnosticResult(EmptyCatchBlockAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Empty catch block – consider rethrowing, logging or documenting why it's empty.")
                .WithSpan(10, 9, 12, 10);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenCatchContainsCodeOrComment() => await VerifyAnalyzerAsync(@"
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
            // intentional
        }
    }
}");
    }
}
