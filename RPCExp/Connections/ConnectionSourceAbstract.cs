using System;
using ModbusBasic.IO;

namespace RPCExp.Connections
{
    public abstract class ConnectionSourceAbstract: INameDescription
    {
        public abstract string ClassName { get; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsOpen => streamResource?.IsOpen ?? false;

        private IStreamResource streamResource;

        public bool EnshureConnected()
        {
            if (!IsOpen)
            {
                try
                {
                    streamResource = TryOpen();
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
            EnshureConnected();

            return streamResource;
        }
        
        protected abstract IStreamResource TryOpen();
    }
}