namespace ExceptionAnalyzer.Sample
{
    internal static class EX025
    {
        public static void Method1()
        {
            Method2(); // EX025: Call may throw 'System.NullReferenceException' as documented, but the exception is not handled here
        }

        /// <summary>
        /// <exception cref="System.NullReferenceException"/>
        /// </summary>
        public static void Method2()
        {
            var i = 0;
            var result = 21 / i;
            Console.WriteLine(result);
        }
    }
}
