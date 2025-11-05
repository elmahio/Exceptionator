namespace ExceptionAnalyzer.Sample
{
    internal static class EX021
    {
        // EX021: Missing expected constructors on custom exception
        public class MyCustomException : Exception
        {
        }
    }
}
