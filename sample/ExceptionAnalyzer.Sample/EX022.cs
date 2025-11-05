namespace ExceptionAnalyzer.Sample
{
    internal static class EX022
    {
        public class MyCustomException : Exception
        {
            // EX022: Exception constructors must call base
            public MyCustomException(string message) { }
            public MyCustomException(string message, Exception inner) { }
        }
    }
}
