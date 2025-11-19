namespace ExceptionAnalyzer.Sample
{
    internal static class EX026
    {
        /// <summary>
        /// Demonstrates missing documentation for explicitly thrown exceptions.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"/>
        public static void Method()
        {
            if (DateTime.Now.Ticks % 2 == 0)
            {
                throw new InvalidOperationException("message");
            }

            // EX026: Method throws 'System.ArgumentException' but does not document this exception, while other exceptions are documented
            throw new ArgumentException("arg");
        }
    }
}
