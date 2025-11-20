using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class LogOnlyExceptionMessageAnalyzerTests : AnalyzerTestBase<LogOnlyExceptionMessageAnalyzer>
    {
        [Test]
        public async Task ReportsDiagnosticWhenOnlyExMessageIsLogged()
        {
            var source = @"
using System;
class C
{
    private readonly ILogger _logger;

    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}

public interface ILogger
{
    void LogError(string message);
}";

            var expected = new DiagnosticResult(LogOnlyExceptionMessageAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Log the entire exception to preserve stack trace and context.")
                .WithSpan(14, 13, 14, 41);

            await VerifyAnalyzerAsync(source, expected);
        }

        [Test]
        public async Task DoesNotReportWhenBothMessageAndExceptionAreLogged() => await VerifyAnalyzerAsync(@"
using System;
class C
{
    private readonly ILogger _logger;

    void M()
    {
        try
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }
}

public interface ILogger
{
    void LogError(string message, Exception exception);
}");

        [Test]
        public async Task DoesNotReportWhenMessageIsUsedInNonLoggingCall() => await VerifyAnalyzerAsync(@"
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
            Save(ex.Message);
        }
    }

    void Save(string message) { }
}");
    }
}
