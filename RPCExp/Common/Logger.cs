using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RPCExp.Common
{


    public static class Logger
    {
        public static bool Enabled { get; set; } = true;

        public enum LogMessageCategory
        {
            Info,
            Warning,
            Error
        }

        public static void Log(string message, [CallerMemberName] string source = "", LogMessageCategory category = LogMessageCategory.Info) {
            if (!Enabled)
                return;

            LogMessageCategory mc = LogMessageCategory.Error | LogMessageCategory.Info;

            var dt = DateTime.Now.ToString();

            Console.WriteLine($"{dt};{source};{category};{message}");
        }
        
    }
}
