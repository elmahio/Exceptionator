namespace ExceptionAnalyzer.Sample
{
    internal static class EX015
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX014");
            }
            catch (InvalidOperationException ex)
            {
                // EX015: Avoid logging ex.ToString()
                LogError("Error: " + ex.ToString());
            }
        }

        private static void LogError(string message)
        {
            Console.WriteLine(message);
        }
    }
}
