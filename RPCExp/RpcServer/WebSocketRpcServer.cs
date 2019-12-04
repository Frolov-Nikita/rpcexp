using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.RpcServer
{
    public class WebSocketRpcServer : WebSocketServerAbstract
    {
        private readonly Router router;

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
                while (socket.State == WebSocketState.Open)
                {
                    var req = await socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

                    // --TODO дочитка для партальных пакетов
                    while (!req.EndOfMessage)
                        await socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

                    var respBytes = await router.Handle(buffer.Array, 0, req.Count).ConfigureAwait(false);

                    //var resp = await router.Handle(buffer.AsSpan(0, r.Count));
                    await socket.SendAsync(respBytes, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (WebSocketException ex)
            {
                socket.Abort();
                System.Diagnostics.Trace.Fail($"{nameof(WebSocketRpcServer)}.SocketHandlerAsync(): {ex.InnerMessage()}");
            }
            socket.Dispose();
        }


    }//class
}
