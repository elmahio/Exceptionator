namespace ExceptionAnalyzer.Sample
{
    internal static class EX001
    {
        public static void Method()
        {
            // EX001: Exception should include a message
            throw new InvalidOperationException();
        }
    }
}
