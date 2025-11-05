#pragma warning disable CS0162 // Unreachable code detected
namespace ExceptionAnalyzer.Sample
{
    internal static class EX006
    {
        public static void Method()
        {
            throw new InvalidOperationException("EX006");
            Console.WriteLine("EX006");
        }
    }
}
#pragma warning restore CS0162 // Unreachable code detected
