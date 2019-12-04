using RPCExp.Common;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.RpcServer
{
    public abstract class WebSocketServerAbstract : ServiceAbstract
    {
        private readonly List<Task> socketsHandlers = new List<Task>(4);
        private readonly string[] hosts;

        public WebSocketServerAbstract(string[] hosts)
        {
            this.hosts = hosts ?? new string[] { "http://localhost:6666/", };
        }

        protected override async Task ServiceTaskAsync(CancellationToken cancellationToken)
        {
            HttpListener httpListener = new HttpListener();

            foreach (var host in hosts)
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
            httpListener.Stop();
            httpListener.Close();

            //httpListener.Dispose(true);
        }


        /// <summary>
        /// Обработка сообщений от подключившегося клиента
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract Task SocketHandlerAsync(WebSocket socket, CancellationToken cancellationToken = default);



    }//class
}
