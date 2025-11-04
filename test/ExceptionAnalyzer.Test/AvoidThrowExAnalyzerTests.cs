using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class AvoidThrowExAnalyzerTests : AnalyzerTestBase<AvoidThrowExAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenUsingThrowEx()
        {
            var throwCode = "throw ex;";
            var source = $@"
using System;
class C
{{
    void M()
    {{
        try
        {{
        }}
        catch (Exception ex)
        {{
            {throwCode}
        }}
    }}
}}";

            const int line = 12;
            const int startCol = 13;
            int endCol = startCol + throwCode.Length;

            var expected = new DiagnosticResult(AvoidThrowExAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Use 'throw;' to preserve stack trace.")
                .WithSpan(line, startCol, line, endCol);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenUsingThrowOnly() => await VerifyAnalyzerAsync(@"
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
            throw;
        }
    }
}");
    }
}
