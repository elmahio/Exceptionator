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
    public class MissingConstructorsAnalyzerTests : AnalyzerTestBase<MissingConstructorsAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenConstructorsAreMissing()
        {
            var source = @"
using System;
class MyException : Exception
{
}";

            var expected = new DiagnosticResult(MissingConstructorsAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Exception 'MyException' must define constructors with (string message) and (string message, Exception innerException).")
                .WithSpan(3, 7, 3, 18)
                .WithArguments("MyException");

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenConstructorsArePresent() => await VerifyAnalyzerAsync(@"
using System;
class MyException : Exception
{
    public MyException(string message) : base(message) { }
    public MyException(string message, Exception innerException) : base(message, innerException) { }
}");
    }
}
