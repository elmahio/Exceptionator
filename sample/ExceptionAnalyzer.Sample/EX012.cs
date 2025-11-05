namespace ExceptionAnalyzer.Sample
{
    internal static class EX012
    {
        // EX012: Don't throw exceptions from property getters
        public static string Name => throw new InvalidOperationException("EX012");
    }
}
