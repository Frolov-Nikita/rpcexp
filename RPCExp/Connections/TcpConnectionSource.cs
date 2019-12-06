using ModbusBasic.IO;
using System.Net.Sockets;

namespace RPCExp.Connections
{
    /// <summary>
    /// Implementation of ConnectionSource for tcp port connections
    /// </summary>
    public class TcpConnectionSource : ConnectionSourceAbstract
    {
        public override string ClassName => "Tcp";

        public int Port { get; set; }

        public string Host { get; set; }

        protected override IStreamResource TryOpen()
        {
            return new TcpClientAdapter(new TcpClient(Host, Port));
        }
    }
}
