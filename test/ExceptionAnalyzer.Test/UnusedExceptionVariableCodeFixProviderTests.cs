using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class UnusedExceptionVariableCodeFixProviderTests : CodeFixTestBase<UnusedExceptionVariableAnalyzer, UnusedExceptionVariableCodeFixProvider>
    {
        [Test]
        public async Task RemovesUnusedExceptionVariable()
        {
            var source = @"
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
        }
    }
}";
            var fixedSource = @"
using System;
class C
{
    void M()
    {
        try
        {
        }
        catch (Exception)
        {
        }
    }
}";

            var expected = new DiagnosticResult(UnusedExceptionVariableAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("The caught exception 'ex' is never used.")
                .WithSpan(10, 26, 10, 28)
                .WithArguments("ex");

            await VerifyCodeFixAsync(source, fixedSource, expected);
        }

        [Test]
        public async Task DoesNothingWhenVariableIsUsed()
        {
            var source = @"
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
            Console.WriteLine(ex.Message);
        }
    }
}";
            await VerifyCodeFixAsync(source, source);
        }
    }
}
