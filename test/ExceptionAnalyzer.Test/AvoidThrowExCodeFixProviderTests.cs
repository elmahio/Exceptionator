using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class AvoidThrowExCodeFixProviderTests
        : CodeFixTestBase<AvoidThrowExAnalyzer, AvoidThrowExCodeFixProvider>
    {
        [Test]
        public async Task ReplacesThrowExWithThrow()
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
            throw;
        }
    }
}";

            const int line = 12;
            const int startCol = 13;
            var endCol = startCol + throwCode.Length;

            var expected = new DiagnosticResult(AvoidThrowExAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Use 'throw;' to preserve stack trace.")
                .WithSpan(line, startCol, line, endCol);

            await VerifyCodeFixAsync(source, fixedSource, expected);
        }

        [Test]
        public async Task DoesNothingWhenAlreadyThrow()
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
            throw;
        }
    }
}";
            await VerifyCodeFixAsync(source, source);
        }
    }
}
