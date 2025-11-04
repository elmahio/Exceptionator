using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using NUnit.Framework;

namespace ExceptionAnalyzer.Test
{
    public class MissingConstructorsCodeFixProviderTests : CodeFixTestBase<MissingConstructorsAnalyzer, MissingConstructorsCodeFixProvider>
    {
        [Test]
        public async Task AddsMissingConstructors()
        {
            var source = @"
using System;
class MyException : Exception
{
}";
            // your code fix adds the two expected ctors at the end of the class
            var fixedSource = @"
using System;
class MyException : Exception
{
    public MyException(string message) : base(message)
    {
    }

    public MyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}";

            var expected = new DiagnosticResult(MissingConstructorsAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
                .WithMessage("Exception 'MyException' must define constructors with (string message) and (string message, Exception innerException).")
                .WithSpan(3, 7, 3, 18)
                .WithArguments("MyException");

            await VerifyCodeFixAsync(source, fixedSource, expected);
        }

        [Test]
        public async Task DoesNothingWhenConstructorsAreAlreadyPresent()
        {
            var source = @"
using System;
class MyException : Exception
{
    public MyException(string message) : base(message) { }
    public MyException(string message, Exception innerException) : base(message, innerException) { }
}";
            await VerifyCodeFixAsync(source, source);
        }
    }
}
