namespace ExceptionAnalyzer.Sample
{
    internal static class EX007
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2737:\"catch\" clauses should do more than rethrow", Justification = "<Pending>")]
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
