using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Helios.Common.Concepts.Services;
using Helios.Common.Logs;
using WebSocketSharp;

namespace Helios.WebSocket.NetworkConnection
{
    public class WebSocketClientService : ThreadedService
    {
        private const int PingInterval = 5000;
        private readonly string serverAddress;
        private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private readonly IConnectionTracker connection;
        private readonly Timer pingTimer;
        private readonly WebSocketSharp.WebSocket ws;
        private readonly AutoResetEvent ping = new AutoResetEvent(false);
        private readonly AutoResetEvent open = new AutoResetEvent(false);
        private readonly AutoResetEvent close = new AutoResetEvent(false);
        private readonly List<IWebSocketClientListener> listeners = new List<IWebSocketClientListener>();

        public WebSocketClientService(string url, ILogger logger, string origin = null, string serviceName = null) : base(serviceName)
        {
            RegisterWaitHandle(ping, PingTick);
            RegisterWaitHandle(open, Ws_OnOpen);
            RegisterWaitHandle(close, Ws_OnClose);
            Logger = logger;
            serverAddress = url;

            ws = new WebSocketSharp.WebSocket(url)
            {
                Origin = origin,
            };

            ws.OnOpen += (a,e) => open.Set();
            ws.OnClose += (a, e) => close.Set();
            ws.OnError += Ws_OnError;
            ws.OnMessage += Ws_OnMessage;

            connection = new ConnectionTracker(() => ws.Connect(), () => ws.Close());

            pingTimer = new Timer(o => ping.Set(), null, Timeout.Infinite, Timeout.Infinite);
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            listeners.ForEach(_ => _.OnMessage(e.Data));
        }

        public void AddListener(IWebSocketClientListener listener)
        {
            listeners.Add(listener);
        }

        private void PingTick()
        {
            if (ws.ReadyState == WebSocketState.Open && connection.IsConnected)
                ws.Ping();
           pingTimer.Change(PingInterval, Timeout.Infinite);
        }

        private void Ws_OnOpen()
        {
            Logger.Log($"Open {Name} {serverAddress}", null);
            connection.OnConnected();

            trigger.Set();
        }

        private void Ws_OnClose()
        {
            Logger.Log($"Closed {Name} {serverAddress}", null);
            connection.OnDisconnected();
        }

        private void Ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Logger.Log($"Error {Name} {serverAddress}", null);
        }

        public override ILogger Logger { get; }

        protected override void OnInitialize()
        {
        }

        protected override void OnStart()
        {
            Debug.Assert(listeners.Any(), "No one listens messages");

            connection.Connect();
            pingTimer.Change(PingInterval, Timeout.Infinite);
        }

        protected override void OnStop()
        {
            connection.Disconnect();
            pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        protected override void Run()
        {
            if (ws.ReadyState != WebSocketState.Open || !connection.IsConnected)
                return;

            string message;
            while (queue.TryDequeue(out message))
            {
                ws.SendAsync(message, b => { });
            }
        }

        public void Send(string message)
        {
            queue.Enqueue(message);
            trigger.Set();
        }
    }

    public interface IWebSocketClientListener
    {
        void OnMessage(string message);
    }
}