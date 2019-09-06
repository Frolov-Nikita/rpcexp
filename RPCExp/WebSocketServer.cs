using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using RPCExp.Common;

namespace RPCExp
{
    public class WebSocketServer:ServiceAbstract
    {
        private HttpListener httpListener;        
        private List<Task> socketsHandlers = new List<Task>(4);
        private Router router;
        private string[] hosts;
        
        public WebSocketServer(Router router, string[] hosts = null)
        {
            this.router = router;
            this.hosts = hosts?? new string[] { "http://localhost:8888/" };
        }


        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            httpListener = new HttpListener();
            foreach(var host in hosts)
                httpListener.Prefixes.Add(host);

            httpListener.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await httpListener.GetContextAsync().ConfigureAwait(false);
                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
                    socketsHandlers.Add(SocketHandlerAsync(webSocketContext.WebSocket, cancellationToken));
                    socketsHandlers.RemoveAll(t => t.Status != TaskStatus.Running);
                }
            }
        }


        /// <summary>
        /// Обработка сообщений от подключившегося клиента
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task SocketHandlerAsync(WebSocket socket, CancellationToken cancellationToken = default)
        {
            try
            {
                var buffer = new ArraySegment<byte>(new byte[16384]);
                while (socket.State == WebSocketState.Open)
                {
                    var req = await socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

                    // TODO дочитка для партальных пакетов
                    //req.EndOfMessage

                    var respBytes = await router.Handle(buffer.Array, 0, req.Count);

                    //var resp = await router.Handle(buffer.AsSpan(0, r.Count));
                    await socket.SendAsync(respBytes, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
                }
            }
            catch {
                socket.Abort();
            }
        }


    }//class
}
