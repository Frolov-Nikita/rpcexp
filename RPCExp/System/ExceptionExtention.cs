using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System
{
    public static class ExceptionExtention
    {
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
