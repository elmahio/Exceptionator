namespace ExceptionAnalyzer.Sample
{
    internal static class EX005
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX005");
            }
            // EX005: Exception variable is unused
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred.");
            }
        }
    }
}
