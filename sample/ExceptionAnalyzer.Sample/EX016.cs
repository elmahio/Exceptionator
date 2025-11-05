#pragma warning disable CS8597 // Thrown value may be null.
namespace ExceptionAnalyzer.Sample
{
    internal static class EX016
    {
        public static void Method()
        {
            // EX016: Avoid throwing null – use a specific exception type instead.
            throw null;
        }
    }
}
#pragma warning restore CS8597 // Thrown value may be null.
