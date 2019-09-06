using ModbusBasic;
using ModbusBasic.IO;
using RPCExp.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RPCExp.Modbus
{

    public enum Physic
    {
        Tcp, Udp, Serial
    }

    public enum FrameType
    {
        Ip,
        Rtu,
        Ascii,
    }

    public interface IPhysicConnectionString 
    {
        IStreamResource FromString(string connString);

        //string cfgString { get; set; }

        string ToCfgString();
    }

    public class TcpConnectionString : IPhysicConnectionString
    {
        public int Port { get; set; }

        public string Host { get; set; }

        public IStreamResource FromString(string connString)
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
    }

    public class UdpConnectionString : IPhysicConnectionString
    {
        public int Port { get; set; }

        public string Host { get; set; }

        public IStreamResource FromString(string connString)
        {
            var csParts = connString.Split(":");
            Host = csParts[0];
            Port = int.Parse(csParts[1]);
            return new UdpClientAdapter(new UdpClient(Host, Port));
        }

        public string ToCfgString()
        {
            return $"{Host}:{Port}";
        }
    }

    public class SerialConnectionString : IPhysicConnectionString
    {
        public string  Port { get; set; }

        public int Baud { get; set; } = 9600;

        public int Data { get; set; } = 8;

        RJCP.IO.Ports.Parity Parity { get; set; } = RJCP.IO.Ports.Parity.None;

        RJCP.IO.Ports.StopBits StopBits { get; set; } = RJCP.IO.Ports.StopBits.One;

        static bool ParityTryParse(string s, out RJCP.IO.Ports.Parity parity)
        {
           switch (s)
            {
                case nameof(RJCP.IO.Ports.Parity.None):
                    parity = RJCP.IO.Ports.Parity.None;
                    return true;

                case nameof(RJCP.IO.Ports.Parity.Odd):
                    parity = RJCP.IO.Ports.Parity.Odd;
                    return true;

                case nameof(RJCP.IO.Ports.Parity.Even):
                    parity = RJCP.IO.Ports.Parity.Even;
                    return true;

                case nameof(RJCP.IO.Ports.Parity.Mark):
                    parity = RJCP.IO.Ports.Parity.Mark;
                    return true;

                case nameof(RJCP.IO.Ports.Parity.Space):
                    parity = RJCP.IO.Ports.Parity.Space;
                    return true;

                default:
                    parity = default;
                    return false;
            }
        }

        static bool StopBitsTryParse(string s, out RJCP.IO.Ports.StopBits stopBits)
        {
            switch (s)
            {
                case nameof(RJCP.IO.Ports.StopBits.One):
                    stopBits = RJCP.IO.Ports.StopBits.One;
                    return true;

                case nameof(RJCP.IO.Ports.StopBits.One5):
                    stopBits = RJCP.IO.Ports.StopBits.One5;
                    return true;

                case nameof(RJCP.IO.Ports.StopBits.Two):
                    stopBits = RJCP.IO.Ports.StopBits.Two;
                    return true;

                default:
                    stopBits = default;
                    return false;
            }
        }

        public IStreamResource FromString(string connString)
        {
            var csParts = connString.Split(";");

            Port = csParts[0];

            if (csParts.Length > 1)
            {
                if (int.TryParse(csParts[1], out int b))
                    Baud = b;

                if (csParts.Length > 2)
                {
                    if (int.TryParse(csParts[2], out int d))
                        Data = d;

                    if (csParts.Length > 3)
                    {
                        if (ParityTryParse(csParts[3], out RJCP.IO.Ports.Parity p))
                            Parity = p;

                        if (csParts.Length > 4)
                        {
                            if (StopBitsTryParse(csParts[4], out RJCP.IO.Ports.StopBits s))
                                StopBits = s;
                        }
                    }
                }
            }

            var sps = new RJCP.IO.Ports.SerialPortStream(Port, Baud, Data, Parity, StopBits);
            sps.Open();
            return  new SerialPortStreamAdapter(sps);
        }

        public string ToCfgString() => $"{Port};{Baud};{Data};{Parity};{StopBits};";
    }

    public class ConnectionSource : INameDescription
    {
        static readonly IDictionary<Physic, IPhysicConnectionString> ConnConvs = new Dictionary<Physic, IPhysicConnectionString>
        {
            [Physic.Serial] = new SerialConnectionString(),
            [Physic.Tcp] = new TcpConnectionString(),
            [Physic.Udp] = new UdpConnectionString(),
        };

        public ConnectionSource(string cfg = default)
        {
            if(cfg != default)
                Cfg = cfg;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public Physic Physic { get; set; }

        // Параметры подключения
        public string ConnectionCfg { get; set; }

        public bool IsOpen => streamResource?.IsOpen ?? false;

        private IStreamResource streamResource;

        public bool TryOpen()
        {
            if (!IsOpen)
            {
                try
                {
                    streamResource = ConnConvs[Physic].FromString(ConnectionCfg);
                }
                catch
                {
                    streamResource?.Dispose();
                    streamResource = null;
                    return false;
                }
                
            }
            return IsOpen;
        }

        public IStreamResource Get()
        {
            TryOpen();

            return streamResource;
        }
        
        // TODO: продумать по луччше 
        public string Cfg {
            get
            {
                return $"{Name};{Physic};{ConnectionCfg}";
            }
            set
            {
                var p = value.Split(';', 3);
                Name = p[0];
                if (Enum.TryParse(typeof(Physic), p[1], true, out object physic))
                    Physic = (Physic)physic;
                ConnectionCfg = p[2];
            }
        }
    }

    public class MasterSource
    {
        private ConnectionSource _connectionSource;

        public FrameType frameType { get; set; }


        private IModbusMaster modbusMaster;
        
        public IModbusMaster Get(IModbusFactory factory, ConnectionSource connectionSource)
        {
            if ((connectionSource == _connectionSource) && 
                (_connectionSource.IsOpen) && 
                (modbusMaster != default) && 
                (modbusMaster?.Transport?.StreamResource?.IsOpen??false))
                return modbusMaster;
                

            _connectionSource = connectionSource;
            var streamResource = _connectionSource.Get();

            if (_connectionSource.Physic == Physic.Tcp || _connectionSource.Physic == Physic.Udp)
            {
                if (frameType == FrameType.Ip)
                    modbusMaster = factory.CreateIpMaster(streamResource);

                if (frameType == FrameType.Rtu)
                {
                    var t = factory.CreateRtuTransport(streamResource);
                    modbusMaster = factory.CreateMaster(t);
                }

                if (frameType == FrameType.Ascii)
                {
                    var t = factory.CreateAsciiTransport(streamResource);
                    modbusMaster = factory.CreateMaster(t);
                }
            }

            if (_connectionSource.Physic == Physic.Serial)
            {
                if (frameType == FrameType.Rtu)
                {
                    var t = factory.CreateRtuTransport(streamResource);
                    modbusMaster = factory.CreateMaster(t);
                }

                if (frameType == FrameType.Ascii)
                {
                    var t = factory.CreateAsciiTransport(streamResource);
                    modbusMaster = factory.CreateMaster(t);
                }
            }

            return modbusMaster;
        }

    }
}
