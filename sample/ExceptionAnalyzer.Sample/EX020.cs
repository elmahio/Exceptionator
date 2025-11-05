namespace ExceptionAnalyzer.Sample
{
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
