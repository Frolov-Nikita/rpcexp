using ModbusBasic.IO;
using System;

namespace RPCExp.Connections
{
    /// <summary>
    /// base class for connection sources.
    /// implementation should contain all connection parameters.
    /// </summary>
    public abstract class ConnectionSourceAbstract : INameDescription
    {
        /// <summary>
        /// Name of the concrete class for deserialization
        /// </summary>
        public abstract string ClassName { get; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <summary>
        /// encapsulate streamResource.IsOpen
        /// </summary>
        public bool IsOpen => streamResource?.IsOpen ?? false;

        private IStreamResource streamResource;

        /// <summary>
        /// Ensures that connection is initialized
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Getting streamSource.
        /// It also tries to open it if it's not opened yet.
        /// </summary>
        /// <returns></returns>
        public IStreamResource Get()
        {
            EnshureConnected();

            return streamResource;
        }

        /// <summary>
        /// Tries to initialize and open connection. 
        /// </summary>
        /// <returns></returns>
        protected abstract IStreamResource TryOpen();
    }
}