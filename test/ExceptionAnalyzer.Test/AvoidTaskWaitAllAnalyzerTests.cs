using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class AvoidTaskWaitAllAnalyzerTests : AnalyzerTestBase<AvoidTaskWaitAllAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenTaskWaitAllIsNotCaughtAsAggregate()
        {
            var waitAllCode = "Task.WaitAll(tasks);";
            var source = $@"
using System;
using System.Threading.Tasks;
class C
{{
    void M(Task[] tasks)
    {{
        try
        {{
            {waitAllCode}
        }}
        catch (Exception)
        {{
        }}
    }}
}}";
            const int line = 10;
            const int startCol = 13;
            int endCol = startCol + waitAllCode.Length - 1;

            var expected = new DiagnosticResult(AvoidTaskWaitAllAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Task.WaitAll should be used with caution – catch AggregateException or use await Task.WhenAll for proper exception handling.")
                .WithSpan(line, startCol, line, endCol);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenAggregateExceptionIsCaught() => await VerifyAnalyzerAsync(@"
using System;
using System.Threading.Tasks;
class C
{
    void M(Task[] tasks)
    {
        try
        {
            Task.WaitAll(tasks);
        }
        catch (AggregateException)
        {
        }
    }
}");
    }
}
