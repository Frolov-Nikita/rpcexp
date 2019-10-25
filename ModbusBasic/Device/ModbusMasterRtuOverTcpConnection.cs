using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ModbusBasic.IO;
using ModbusBasic.Message;

namespace ModbusBasic.Device
{
    using Extensions;

    /// <summary>
    /// Represents an incoming connection from a Modbus master. Contains the slave's logic to process the connection.
    /// </summary>
    internal class ModbusMasterRtuOverTcpConnection : ModbusDevice, IDisposable
    {
        private readonly IModbusSlaveNetwork _slaveNetwork;
        private readonly IModbusFactory _modbusFactory;
        private readonly Task _requestHandlerTask;

        private readonly byte[] _mbapHeader = new byte[6];
        private byte[] _messageFrame;

        public ModbusMasterRtuOverTcpConnection(TcpClient client, IModbusSlaveNetwork slaveNetwork, IModbusFactory modbusFactory)
            : base(new ModbusRtuTransport(new TcpClientAdapter(client), modbusFactory))
        {
            //Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            TcpClient = client ?? throw new ArgumentNullException(nameof(client));
            EndPoint = client.Client.RemoteEndPoint.ToString();
            Stream = client.GetStream();
            _slaveNetwork = slaveNetwork ?? throw new ArgumentNullException(nameof(slaveNetwork));
            _modbusFactory = modbusFactory ?? throw new ArgumentNullException(nameof(modbusFactory));
            _requestHandlerTask = Task.Run((Func<Task>)HandleRequestAsync);
        }

        /// <summary>
        ///     Occurs when a Modbus master TCP connection is closed.
        /// </summary>
        public event EventHandler<TcpConnectionEventArgs> ModbusMasterTcpConnectionClosed;



        public string EndPoint { get; }

        public Stream Stream { get; }

        public TcpClient TcpClient { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stream.Dispose();
            }

            base.Dispose(disposing);
        }

        private async Task HandleRequestAsync()
        {

            var serialTransport = (IModbusSerialTransport)Transport;

            while (true)
            {
                try
                {
                    // read request and build message
                    byte[] frame = serialTransport.ReadRequest();

                    //Create the request
                    IModbusMessage request = _modbusFactory.CreateModbusRequest(frame);

                    //Check the message
                    if (serialTransport.CheckFrame && !serialTransport.ChecksumsMatch(request, frame))
                    {
                        string msg = $"Checksums failed to match {string.Join(", ", request.MessageFrame)} != {string.Join(", ", frame)}.";
                        //Logger.Warning(msg);
                        throw new IOException(msg);
                    }

                    //Apply the request
					// TODO: почистить класс и предусмотреть броадкаст request.SlaveAddress ==0
                    IModbusMessage response = _slaveNetwork.GetSlave(request.SlaveAddress)
                        .ApplyRequest(request);

                    if (response == null)
                    {
                        serialTransport.IgnoreResponse();
                    }
                    else
                    {
                        Transport.Write(response);
                    }
                }
                catch (IOException /*ioe*/)
                {
                    //Logger.Warning($"IO Exception encountered while listening for requests - {ioe.Message}");
                    serialTransport.DiscardInBuffer();
                }
                catch (TimeoutException /*te*/)
                {
                    //Logger.Trace($"Timeout Exception encountered while listening for requests - {te.Message}");
                    serialTransport.DiscardInBuffer();
                }
                catch (InvalidOperationException)
                {
                    // when the underlying transport is disposed
                    break;
                }
                catch (Exception /*ex*/)
                {
                    //Logger.Error($"{GetType()}: {ex.Message}");
                    serialTransport.DiscardInBuffer();
                }

                /* // OLD CODE
                // //Logger.Debug($"Begin reading header from Master at IP: {EndPoint}");

                int readBytes = await Stream.ReadAsync(_mbapHeader, 0, 6).ConfigureAwait(false);
                if (readBytes == 0)
                {
                    //Logger.Debug($"0 bytes read, Master at {EndPoint} has closed Socket connection.");
                    ModbusMasterTcpConnectionClosed?.Invoke(this, new TcpConnectionEventArgs(EndPoint));
                    return;
                }

                ushort frameLength = (ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(_mbapHeader, 4));
                //Logger.Debug($"Master at {EndPoint} sent header: \"{string.Join(", ", _mbapHeader)}\" with {frameLength} bytes in PDU");

                _messageFrame = new byte[frameLength];
                readBytes = await Stream.ReadAsync(_messageFrame, 0, frameLength).ConfigureAwait(false);
                if (readBytes == 0)
                {
                    //Logger.Debug($"0 bytes read, Master at {EndPoint} has closed Socket connection.");
                    ModbusMasterTcpConnectionClosed?.Invoke(this, new TcpConnectionEventArgs(EndPoint));
                    return;
                }

                //Logger.Debug($"Read frame from Master at {EndPoint} completed {readBytes} bytes");
                byte[] frame = _mbapHeader.Concat(_messageFrame).ToArray();
                //Logger.Trace($"RX from Master at {EndPoint}: {string.Join(", ", frame)}");

                var request = _modbusFactory.CreateModbusRequest(_messageFrame);
                request.TransactionId = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(frame, 0));

                IModbusSlave slave = _slaveNetwork.GetSlave(request.SlaveAddress);

                if (slave != null)
                {
                    //TODO: Determine if this is appropriate

                    // perform action and build response
                    IModbusMessage response = slave.ApplyRequest(request);
                    response.TransactionId = request.TransactionId;

                    // write response
                    byte[] responseFrame = Transport.BuildMessageFrame(response);
                    //Logger.Information($"TX to Master at {EndPoint}: {string.Join(", ", responseFrame)}");
                    await Stream.WriteAsync(responseFrame, 0, responseFrame.Length).ConfigureAwait(false);
                }
                */
            }
        }
    }
}
