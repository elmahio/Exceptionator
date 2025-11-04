using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class ExceptionShouldBePublicCodeFixProviderTests : CodeFixTestBase<ExceptionShouldBePublicAnalyzer, ExceptionShouldBePublicCodeFixProvider>
    {
        [Test]
        public async Task MakesExceptionPublic()
        {
            var source = @"
using System;
class MyException : Exception
{
}";
            var fixedSource = @"
using System;
public class MyException : Exception
{
}";

            var expected = new DiagnosticResult(ExceptionShouldBePublicAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Exception class 'MyException' should be declared public.")
                .WithSpan(3, 7, 3, 18)
                .WithArguments("MyException");

            await VerifyCodeFixAsync(source, fixedSource, expected);
        }

        [Test]
        public async Task DoesNothingWhenAlreadyPublic()
        {
            var source = @"
using System;
public class MyException : Exception
{
}";
            await VerifyCodeFixAsync(source, source);
        }
    }
}
