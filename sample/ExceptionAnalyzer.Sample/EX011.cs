namespace ExceptionAnalyzer.Sample
{
    internal static class EX011
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX011");
            }
            // EX011: Empty catch block
            catch (InvalidOperationException)
            {
            }
        }
    }
}
