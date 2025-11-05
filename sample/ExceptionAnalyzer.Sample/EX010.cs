namespace ExceptionAnalyzer.Sample
{
    internal static class EX010
    {
        public static void Method()
        {
            var tasks = new Task[]
            {
                Task.Run(() => { }),
            };

            try
            {
                // EX010: Task.WaitAll should be wrapped with AggregateException catch
                Task.WaitAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
