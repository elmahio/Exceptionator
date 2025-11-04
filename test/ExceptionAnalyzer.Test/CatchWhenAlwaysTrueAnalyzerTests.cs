using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class CatchWhenAlwaysTrueAnalyzerTests : AnalyzerTestBase<CatchWhenAlwaysTrueAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenWhenClauseAlwaysTrue()
        {
            var whenCode = "when (true)";
            var source = $@"
using System;
class C
{{
    void M()
    {{
        try
        {{
        }}
        catch (Exception) {whenCode}
        {{
        }}
    }}
}}";

            const int line = 10;
            const int startCol = 27;
            var endCol = startCol + whenCode.Length;

            var expected = new DiagnosticResult(CatchWhenAlwaysTrueAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("The 'when' filter always evaluates to true and is redundant.")
                .WithSpan(line, startCol, line, endCol);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenWhenClauseNotConstantTrue() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    void M()
    {
        try
        {
        }
        catch (Exception ex) when (ex.Message.Length > 0)
        {
        }
    }
}");
    }
}
