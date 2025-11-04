using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionAnalyzer.Test
{
    public class ThrowNullAnalyzerTests
        : AnalyzerTestBase<ThrowNullAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenThrowingNull()
        {
            var source = @"
class C
{
    void M()
    {
        throw null;
    }
}";

            var expected = new DiagnosticResult(ThrowNullAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Avoid throwing null – use a specific exception type instead.")
                .WithSpan(6, 9, 6, 20);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenThrowingException() => await VerifyAnalyzerAsync(@"
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
