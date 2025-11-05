namespace ExceptionAnalyzer.Sample
{
    internal static class EX019
    {
        public static void DoWork() => throw new NotImplementedException("No implemented");

        public static void Method()
        {
            // EX019: NotImplementedException left in code
            throw new NotImplementedException("EX019");
        }
    }
}
