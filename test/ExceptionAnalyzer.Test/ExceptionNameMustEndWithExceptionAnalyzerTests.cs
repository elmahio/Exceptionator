using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ExceptionNameMustEndWithExceptionAnalyzerTests : AnalyzerTestBase<ExceptionNameMustEndWithExceptionAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenExceptionNameDoesNotEndWithException()
        {
            var classDeclaration = "class MyError : Exception";
            var source = $@"
using System;
{classDeclaration}
{{
}}";

            var expected = new DiagnosticResult(ExceptionNameMustEndWithExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Class 'MyError' inherits from System.Exception but does not end with 'Exception'.")
                .WithSpan(3, 7, 3, 14);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenExceptionNameEndsWithException() => await VerifyAnalyzerAsync(@"
using System;
class MyCustomException : Exception
{
}");
    }
}
