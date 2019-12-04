namespace System
{
    /// <summary>
    /// Some useful extension methods for exceptions
    /// </summary>
    public static class ExceptionExtension
    {
        /// <summary>
        /// Gets the base exception & make string
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="traceMethod">if need to add method name from stack trace</param>
        /// <returns>string of base inner exception message and method name from stack trace if available</returns>
        public static string InnerMessage(this Exception exception, bool traceMethod = true)
        {
#pragma warning disable CA1062 // Проверить аргументы или открытые методы
            var e = exception.GetBaseException();
#pragma warning restore CA1062 // Проверить аргументы или открытые методы

            var m = e.Message;
            if (traceMethod)
                m += $" in method: {e.Source}";

            return m;
        }
    }
}
