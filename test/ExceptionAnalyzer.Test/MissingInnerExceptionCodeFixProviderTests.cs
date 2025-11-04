using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class MissingInnerExceptionCodeFixProviderTests : CodeFixTestBase<MissingInnerExceptionAnalyzer, MissingInnerExceptionCodeFixProvider>
    {
        [Test]
        public async Task AddsInnerExceptionArgument()
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
            throw new InvalidOperationException(""msg"");
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
        catch (Exception ex)
        {
            throw new InvalidOperationException(""msg"", ex);
        }
    }
}";

            var expected = new DiagnosticResult(MissingInnerExceptionAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("New exception should include the caught exception as inner exception.")
                .WithSpan(12, 19, 12, 55);

            await VerifyCodeFixAsync(source, fixedSource, expected);
        }

        [Test]
        public async Task DoesNothingWhenInnerExceptionAlreadyPresent()
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
            throw new InvalidOperationException(""msg"", ex);
        }
    }
}";
            await VerifyCodeFixAsync(source, source);
        }
    }
}
