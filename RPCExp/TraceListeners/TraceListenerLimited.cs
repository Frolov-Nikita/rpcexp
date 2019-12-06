using System.Collections.Generic;
using System.Diagnostics;

namespace RPCExp.TraceListeners
{

    /// <summary>
    /// Listens Trace and debug messages from System.Diagnostics.Trace / Debug
    /// Stores it in limited Observable collection
    /// To get new messages you need to subscribe to Messages.CollectionChanged event
    /// </summary>
    internal class TraceListenerLimited : TraceListener
    {
        public LimitedObservableCollection<TraceMessage> Messages { get; } = new LimitedObservableCollection<TraceMessage> { Limit = 10 };

        private TraceListenerLimited()
        {
            Trace.Listeners.Add(this);
        }

        private static readonly TraceListenerLimited instatce = new TraceListenerLimited();

        public static TraceListenerLimited GetConnectedInstatce() => instatce;

        private void Add(TraceMessage message)
        {
            Messages.Add(message);
            
        }

        public override void Fail(string message)
        {
            Add(new TraceMessage
            {
                Categoty = "Fail",
                Message = message,
            });
        }

        public override void Fail(string message, string detailMessage)
        {
            Add(new TraceMessage
            {
                Categoty = "Fail",
                Message = message,
                Detail = detailMessage,
            });
        }

        public override void Write(string message, string category)
        {
            Add(new TraceMessage
            {
                Categoty = category,
                Message = message,
            });
        }

        public override void Write(string message)
        {
            Add(new TraceMessage
            {
                Message = message,
            });
        }

        public override void WriteLine(string message)
        {
            Add(new TraceMessage
            {
                Message = message,
            });
        }

    }
}
