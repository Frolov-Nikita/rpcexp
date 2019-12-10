using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.RpcServer
{
    /// <summary>
    /// main class to provide data
    /// </summary>
    public class WebSocketRpcServer : WebSocketServerAbstract
    {
        private readonly Router router;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="router">see Router description</param>
        /// <param name="hosts">should be localhost! and loks like new string[] { "http://localhost:8888/" }</param>
        public WebSocketRpcServer(Router router, string[] hosts = null)
            : base(hosts ?? new string[] { "http://localhost:8888/" })
        {
            this.router = router;
        }

        /// <summary>
        /// Обработка сообщений от подключившегося клиента
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task SocketHandlerAsync(WebSocket socket, CancellationToken cancellationToken = default)
        {
            if (socket is null)
                throw new ArgumentNullException(nameof(socket));

            try
            {
                var buffer = new ArraySegment<byte>(new byte[16384]);
                //if (socket.State == WebSocketState.Open)
                while ((socket.State == WebSocketState.Open) && (!cancellationToken.IsCancellationRequested))
                {
                    var req = await socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

                    //while (!req.EndOfMessage)
                    //    await socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
                    
                    if ((socket.State != WebSocketState.Open) || (cancellationToken.IsCancellationRequested))
                        break;

                    var respBytes = await router.Handle(buffer.Array, 0, req.Count).ConfigureAwait(false);

                    if ((socket.State != WebSocketState.Open) || (cancellationToken.IsCancellationRequested))
                        break;

                    await socket.SendAsync(respBytes, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"{nameof(WebSocketRpcServer)}.SocketHandlerAsync(): {ex.InnerMessage()}");
            }
            
            socket.Abort();
            socket.Dispose();
        }


    }//class
}
