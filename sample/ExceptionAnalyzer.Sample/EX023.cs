namespace ExceptionAnalyzer.Sample
{
    internal static class EX023
    {
        // EX023: Exception class name must end with 'Exception'
        public class MyCustomError : Exception
        {
            public MyCustomError(string message) : base(message) { }
            public MyCustomError(string message, Exception inner) : base(message, inner) { }
        }
    }
}
