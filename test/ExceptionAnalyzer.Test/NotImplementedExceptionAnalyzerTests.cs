using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class NotImplementedExceptionAnalyzerTests : AnalyzerTestBase<NotImplementedExceptionAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenThrowingNotImplementedException()
        {
            var source = @"
using System;
class C
{
    void M()
    {
        throw new NotImplementedException();
    }
}";

            var expected = new DiagnosticResult(NotImplementedExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Avoid leaving 'throw new NotImplementedException()' in production code.")
                .WithSpan(7, 9, 7, 45);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticForOtherThrows() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    void M()
    {
        throw new InvalidOperationException();
    }
}");
    }
}
