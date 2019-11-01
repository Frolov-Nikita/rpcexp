using ModbusBasic;
using RPCExp.Connections;

namespace RPCExp.Modbus
{
    public class MasterSource
    {
        private ConnectionSourceAbstract _connectionSource;

        private IModbusMaster modbusMaster;
        
        public IModbusMaster Get(IModbusFactory factory, FrameType frameType, ConnectionSourceAbstract connectionSource)
        {
            if ((connectionSource == _connectionSource) && 
                (_connectionSource.IsOpen) && 
                (modbusMaster != default) && 
                (modbusMaster?.Transport?.StreamResource?.IsOpen??false))
                return modbusMaster;
            
            _connectionSource = connectionSource;
            var streamResource = _connectionSource.Get();

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

            return modbusMaster;
        }

    }
}
