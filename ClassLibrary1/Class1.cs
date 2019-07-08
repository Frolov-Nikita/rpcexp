using System;
using System.IO.Ports;

namespace ClassLibrary1
{
    public class Class1
    {
        public static string Get()
        {
            var ports = SerialPort.GetPortNames();
            string s = "";
            foreach (var n in ports)
                s += n + "\r\n";
            return s;
        }
    }
}
