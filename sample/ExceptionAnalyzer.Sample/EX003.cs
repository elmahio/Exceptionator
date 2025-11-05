namespace ExceptionAnalyzer.Sample
{
    internal static class EX003
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX003");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // EX003: Missing inner exception
                throw new InvalidOperationException("Something failed");
            }
        }
    }
}
