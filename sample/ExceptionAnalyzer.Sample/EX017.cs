#pragma warning disable CS7095 // Filter expression is a constant 'true'
namespace ExceptionAnalyzer.Sample
{
    internal static class EX017
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX017");
            }
            // EX017: Avoid when clauses that always evaluate to true
            catch (Exception ex) when (true)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
#pragma warning restore CS7095 // Filter expression is a constant 'true'
