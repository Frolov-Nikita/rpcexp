using RPCExp.RpcServer;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RPCExp.TraceListeners
{
    public class WebSocketTraceServer : WebSocketServerAbstract
    {
        private List<WebSocket> webSockets = new List<WebSocket>();
        private LimitedObservableCollection<TraceMessage> messages;

        public WebSocketTraceServer(string[] hosts = default)
            : base(hosts ?? new string[] { "http://*:7777/" })
        {
            messages = TraceListenerLimited.GetConnectedInstatce().Messages;
            messages.CollectionChanged += handler;
        }

        protected override async Task SocketHandlerAsync(WebSocket socket, CancellationToken cancellationToken = default)
        {
            if (socket is null)
                throw new ArgumentNullException(nameof(socket));

            var socketIsOk = true;
            foreach (var m in messages)
            {
                var buffer = Encoding.UTF8.GetBytes(m.ToString());
                socketIsOk &= await WriteToSocket(buffer, socket, cancellationToken).ConfigureAwait(false);
            }

            if (socketIsOk)
                webSockets.Add(socket);
        }

        private async void handler(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if ((e?.NewItems?.Count ?? 0) == 0)
                return;

            var sockets = webSockets.FindAll(s => s.State == WebSocketState.Open);

            if ((sockets?.Count ?? 0) == 0)
                return;

            var tasks = new List<Task<bool>>(sockets.Count);

            foreach (var m in e.NewItems)
            {
                var buffer = Encoding.UTF8.GetBytes(((TraceMessage)m).ToString());
                foreach (var socket in sockets)
                    tasks.Add(WriteToSocket(buffer, socket));

                await Task.WhenAll(tasks).ConfigureAwait(false);
                tasks.Clear();
            }

            // remove closed sockets
            sockets = webSockets.FindAll(s => s.State != WebSocketState.Open);
            foreach (var socket in sockets)
            {
                socket.Abort();
                socket.Dispose();
                webSockets.Remove(socket);
            }

        }

        private static async Task<bool> WriteToSocket(byte[] buffer, WebSocket socket, CancellationToken cancellationToken = default)
        {
            if (socket?.State == WebSocketState.Open)
                try
                {
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
                    return true;
                }
                catch// (System.Net.WebSockets.WebSocketException ex)
                {
                    socket.Abort();
                    socket.Dispose();
                }
            return false;
        }
    }
}
