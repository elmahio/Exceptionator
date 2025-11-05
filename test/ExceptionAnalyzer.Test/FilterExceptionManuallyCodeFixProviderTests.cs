using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class FilterExceptionManuallyCodeFixProviderTests : CodeFixTestBase<FilterExceptionManuallyAnalyzer, FilterExceptionManuallyCodeFixProvider>
    {
        [Test]
        public async Task IntroducesSpecificCatchAndRemovesIf()
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
            if (ex is InvalidOperationException)
            {
                Console.WriteLine(""invalid"");
            }
            else
            {
                Console.WriteLine(ex.Message);
            }
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
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(""invalid"");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}";

            var expected = new DiagnosticResult(FilterExceptionManuallyAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("Use exception filters or catch specific exception types instead of 'if (ex is ...)'.")
                .WithSpan(12, 13, 19, 14);

            await VerifyCodeFixAsync(source, fixedSource, expected);
        }

        [Test]
        public async Task DoesNothingWhenNoPatternFound()
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
