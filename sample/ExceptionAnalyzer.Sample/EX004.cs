namespace ExceptionAnalyzer.Sample
{
    internal static class EX004
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX004");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // EX004: Use 'throw;' instead of 'throw ex;'
                throw ex;
            }
        }
    }
}
