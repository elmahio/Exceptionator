using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class AvoidBaseExceptionAnalyzerTests : AnalyzerTestBase<AvoidBaseExceptionAnalyzer>
    {
        [TestCase(typeof(Exception))]
        [TestCase(typeof(SystemException))]
        public async Task ReportsDiagnosticWhenThrowingBaseException(Type exception)
        {
            var throwText = $"new {exception.FullName}();";
            var source = $@"
using System;
class C
{{
    void M()
    {{
        throw {throwText}
    }}
}}";

            const int line = 7;
            const int startCol = 15;
            var endCol = startCol + throwText.Length - 1;

            var expected = new DiagnosticResult(AvoidBaseExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage($"Throwing '{exception.Name}' is discouraged. Use a more specific exception type.")
                .WithSpan(line, startCol, line, endCol)
                .WithArguments(exception.Name);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticForSpecificException() => await VerifyAnalyzerAsync(@"
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
