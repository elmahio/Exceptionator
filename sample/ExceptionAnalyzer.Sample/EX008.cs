namespace ExceptionAnalyzer.Sample
{
    internal static class EX008
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX008");
            }
            // EX008: ThreadAbortException must not be swallowed
            catch (ThreadAbortException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
