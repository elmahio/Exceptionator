#pragma warning disable CS0168 // Variable is declared but never used
namespace ExceptionAnalyzer.Sample
{
    internal static class EX005
    {
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
#pragma warning restore CS0168 // Variable is declared but never used
