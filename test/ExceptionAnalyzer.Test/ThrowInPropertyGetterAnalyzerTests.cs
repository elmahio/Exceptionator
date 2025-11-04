using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ThrowInPropertyGetterAnalyzerTests : AnalyzerTestBase<ThrowInPropertyGetterAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenGetterThrows()
        {
            var source = @"
using System;
class C
{
    public int P
    {
        get
        {
            throw new Exception();
        }
    }
}";
            var expected = new DiagnosticResult(ThrowInPropertyGetterAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Avoid throwing exceptions from property getters.")
                .WithSpan(9, 13, 9, 35);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenGetterDoesNotThrow() => await VerifyAnalyzerAsync(@"
class C
{
    public int P { get; } = 42;
}");
    }
}
