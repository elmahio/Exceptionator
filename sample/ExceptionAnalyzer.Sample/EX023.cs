namespace ExceptionAnalyzer.Sample
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3376:Attribute, EventArgs, and Exception type names should end with the type being extended", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>")]
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
