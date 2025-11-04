using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class CatchExceptionAndTypeCheckCodeFixProviderTests : CodeFixTestBase<CatchExceptionAndTypeCheckAnalyzer, CatchExceptionAndTypeCheckCodeFixProvider>
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

            Console.WriteLine(ex.Message);
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

            var expected = new DiagnosticResult(CatchExceptionAndTypeCheckAnalyzer.DiagnosticId, DiagnosticSeverity.Info)
                .WithMessage("This catch handles 'Exception' but checks for 'InvalidOperationException'. Consider adding a dedicated catch clause.")
                .WithSpan(12, 13, 12, 15);

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
