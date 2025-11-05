namespace ExceptionAnalyzer.Sample
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>")]
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
