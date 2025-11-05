namespace ExceptionAnalyzer.Sample
{
    internal static class EX009
    {
        public static void Method()
        {
            // EX009: Empty try block with catch
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
