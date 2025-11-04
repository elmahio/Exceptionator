using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ExceptionConstructorShouldCallBaseCorrectlyAnalyzerTests : AnalyzerTestBase<ExceptionConstructorShouldCallBaseCorrectlyAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenConstructorDoesNotCallBase()
        {
            var source = @"
using System;
class MyException : Exception
{
    public MyException(string message)
    {
    }
}";

            var expected = new DiagnosticResult(ExceptionConstructorShouldCallBaseCorrectlyAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Constructor should pass 'message' to base constructor")
                .WithSpan(5, 12, 5, 23);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenConstructorCallsBaseCorrectly() => await VerifyAnalyzerAsync(@"
using System;
class MyException : Exception
{
    public MyException(string message) : base(message)
    {
    }
}");
    }
}
