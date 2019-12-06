using ModbusBasic.IO;
using System.Net.Sockets;

namespace RPCExp.Connections
{
    /// <summary>
    /// Implementation of ConnectionSource for udp port connections
    /// </summary>
    public class UdpConnectionSource : ConnectionSourceAbstract
    {
        public override string ClassName => "Udp";

        public int Port { get; set; }

        public string Host { get; set; }


        protected override IStreamResource TryOpen()
        {
            return new UdpClientAdapter(new UdpClient(Host, Port));
        }
    }
}
