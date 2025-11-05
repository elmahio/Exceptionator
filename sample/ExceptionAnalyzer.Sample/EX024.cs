namespace ExceptionAnalyzer.Sample
{
    internal static class EX024
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX024");
            }
            // EX024: Avoid catching fatal exceptions like StackOverflowException or ExecutionEngineException
            catch (StackOverflowException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
