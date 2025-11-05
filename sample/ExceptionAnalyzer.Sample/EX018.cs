namespace ExceptionAnalyzer.Sample
{
    internal static class EX018
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX018");
            }
            catch (Exception ex)
            {
                // EX018: Filter exceptions manually inside catch
                if (ex is ArgumentException)
                    Console.WriteLine("EX018");
            }
        }
    }
}
