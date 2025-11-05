namespace ExceptionAnalyzer.Sample
{
    internal static class EX014
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX014");
            }
            catch (InvalidOperationException ex)
            {
                // EX014: Avoid logging only ex.Message
                LogError(ex.Message);
            }
        }

        private static void LogError(string message)
        {
            Console.WriteLine(message);
        }
    }
}
