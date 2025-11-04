using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class AvoidCatchingFatalExceptionsAnalyzerTests : AnalyzerTestBase<AvoidCatchingFatalExceptionsAnalyzer>
    {
        [TestCase(typeof(StackOverflowException))]
#pragma warning disable CS0618 // Type or member is obsolete
        [TestCase(typeof(ExecutionEngineException))]
#pragma warning restore CS0618 // Type or member is obsolete
        [TestCase(typeof(AccessViolationException))]
        [TestCase(typeof(OutOfMemoryException))]
        public async Task ReportsDiagnosticWhenCatchingFatalException(Type exception)
        {
            var typeText = exception.FullName!;

            var source = $@"
using System;
class C
{{
    void M()
    {{
        try
        {{
        }}
        catch ({typeText})
        {{
        }}
    }}
}}";

            const int line = 10;
            const int startCol = 16;
            var endCol = startCol + typeText.Length;

            var expected = new DiagnosticResult(AvoidCatchingFatalExceptionsAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage($"Catching '{exception.Name}' is discouraged or has no effect. These exceptions should not be caught.")
                .WithSpan(line, startCol, line, endCol)
                .WithArguments(exception.Name);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportDiagnosticWhenCatchingNormalException() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    void M()
    {
        try
        {
        }
        catch (InvalidOperationException)
        {
        }
    }
}");
    }
}
