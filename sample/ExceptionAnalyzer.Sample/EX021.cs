namespace ExceptionAnalyzer.Sample
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>")]
    internal static class EX021
    {
        // EX021: Missing expected constructors on custom exception
        public class MyCustomException : Exception
        {
        }
    }
}
