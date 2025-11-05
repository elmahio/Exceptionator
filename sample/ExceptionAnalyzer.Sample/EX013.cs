#pragma warning disable CS8597 // Thrown value may be null.
namespace ExceptionAnalyzer.Sample
{
    internal static class EX013
    {
        public static void Method()
        {
            try
            {
                Console.WriteLine("EX013");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // EX013: Avoid throwing ex.InnerException
                throw ex.InnerException;
            }
        }
    }
}
#pragma warning restore CS8597 // Thrown value may be null.
