namespace ExceptionAnalyzer.Sample
{
    internal static class EX007
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX007");
            }
            // EX007: Pointless try/catch block
            catch (Exception)
            {
                throw;
            }
        }
    }
}
