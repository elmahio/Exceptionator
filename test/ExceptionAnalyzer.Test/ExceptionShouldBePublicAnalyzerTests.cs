using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ExceptionShouldBePublicAnalyzerTests : AnalyzerTestBase<ExceptionShouldBePublicAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenExceptionIsNotPublic()
        {
            var source = @"
using System;
class MyException : Exception
{
}";

            var expected = new DiagnosticResult(ExceptionShouldBePublicAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Exception class 'MyException' should be declared public.")
                .WithSpan(3, 7, 3, 18)
                .WithArguments("MyException");

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenExceptionIsPublic() => await VerifyAnalyzerAsync(@"
using System;
public class MyException : Exception
{
}");
    }
}
