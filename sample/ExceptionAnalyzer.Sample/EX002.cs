namespace ExceptionAnalyzer.Sample
{
    internal static class EX002
    {
        public static void Method()
        {
            // EX002: Avoid throwing base exceptions
            throw new Exception("Something went wrong");
        }
    }
}
