using ModbusBasic.IO;
using System;
using System.Net.Sockets;

namespace RPCExp.Connections
{
    public class TcpConnectionString : IConnectionSource
    {
        public int Port { get; set; }

        public string Host { get; set; }

        public bool IsOpen => throw new NotImplementedException();

        public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsValid => throw new NotImplementedException();

        public IStreamResource UpdateFromCfgString(string connString)
        {
            var csParts = connString.Split(":");
            Host = csParts[0];
            Port = int.Parse(csParts[1]);
            return new TcpClientAdapter(new TcpClient(Host, Port));
        }

        public string ToCfgString()
        {
            return $"{Host}:{Port}";
        }

        public IStreamResource Get()
        {
            throw new NotImplementedException();
        }

        public bool TryOpen()
        {
            throw new NotImplementedException();
        }

        void IConfigString.UpdateFromCfgString(string cfg)
        {
            throw new NotImplementedException();
        }
    }
}
