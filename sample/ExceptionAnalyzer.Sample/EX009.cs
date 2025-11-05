namespace ExceptionAnalyzer.Sample
{
    internal static class EX009
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty", Justification = "<Pending>")]
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
