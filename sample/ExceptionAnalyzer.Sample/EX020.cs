namespace ExceptionAnalyzer.Sample
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "<Pending>")]
    internal static class EX020
    {
        // EX020: Exception class should be public
        class CustomException : Exception
        {
            public CustomException(string message) : base(message)
            {
            }

            public CustomException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
