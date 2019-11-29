using System;

namespace RPCExp.TraceListeners
{
    public class TraceMessage
    {
        public DateTime DateTime { get; } = DateTime.Now;

        public string Categoty { get; set; } = "Info";

        public string Message { get; set; } = "";

        public string Detail { get; set; } = "";

        public override string ToString()
        {
#pragma warning disable CA1305 // Укажите IFormatProvider
            var t = DateTime.ToString("yyyy.MM.dd HH:mm:ss.ffff");
#pragma warning restore CA1305 // Укажите IFormatProvider

            return $"{t}: {Categoty}: {Message}. {Detail}";
        }
    }
}
