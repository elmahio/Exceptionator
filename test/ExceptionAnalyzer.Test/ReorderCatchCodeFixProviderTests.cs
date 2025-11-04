using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ReorderCatchCodeFixProviderTests : CodeFixTestBase<NoopAnalyzer, ReorderCatchCodeFixProvider>
    {
        [Test]
        public async Task ReordersWhenCompilerReportsCS0160()
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
        catch (InvalidOperationException ex)
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
        catch (InvalidOperationException ex)
        {
        }
        catch (Exception ex)
        {
        }
    }
}";

            var expected = DiagnosticResult.CompilerError("CS0160")
                .WithSpan(13, 16, 13, 41);

            await VerifyCodeFixAsync(source, fixedSource, expected);
        }

        [Test]
        public async Task DoesNothingWhenNoCS0160()
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
        catch (InvalidOperationException ex)
        {
        }
        catch (Exception ex)
        {
        }
    }
}";
            await VerifyCodeFixAsync(source, source);
        }
    }
}
