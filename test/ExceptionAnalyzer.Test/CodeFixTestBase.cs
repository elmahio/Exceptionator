using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace ExceptionAnalyzer.Test
{
    public abstract class CodeFixTestBase<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        protected async Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
        {
            var test = new CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
            {
                TestCode = source,
                FixedCode = fixedSource
            };

            test.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }
    }
}
