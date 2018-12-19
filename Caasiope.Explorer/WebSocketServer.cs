using System;
using System.Collections.Generic;
using Helios.Common.Extensions;
using Helios.WebSocket;

namespace Caasiope.Explorer
{
	public interface IWebSocketServer
	{
		void OnNewSession(Action<ISession> callback);
		void OnReceive(Action<ISession, string> callback);
		void Start();
		void Stop();
		void Initialize();
		void Send(ISession session, string message);
		void Broadcast(string message);
    }

    public interface ISession
    {
        void Send(string message);
    }

    public class Session : Helios.WebSocket.WebSocketServer.Session, ISession
	{
		public readonly Guid SessionId = Guid.NewGuid();

		void ISession.Send(string message)
		{
			SendAsync(message, delegate {  });
		}

	    public override bool Equals(object obj)
	    {
	        return obj is Session session && SessionId == session.SessionId;
	    }

        public override int GetHashCode()
        {
            return SessionId.GetHashCode();
        }
    }

	public class WebSocketServer : IWebSocketServer
	{
        private readonly Helios.WebSocket.WebSocketServer server;
		private Action<ISession> onNewSession;
		private Action<ISession, string> onReceive;
		
		public WebSocketServer(string path)
		{
			var configuration = WebSocketServerConfiguration.LoadConfiguration(path);
			server = Helios.WebSocket.WebSocketServer.CreateNew(configuration, CreateSession);
		}

		private Helios.WebSocket.WebSocketServer.Session CreateSession()
		{
			var session = new Session();
			session.onOpen += () => OnOpen(session);
			session.onClose += () => OnClose(session);
			session.onMessage += (data) => OnMessage(session, data);
			return session;
		}

		private void OnMessage(Session session, string data)
		{
			onReceive.Call(session, data);
		}

		private void OnClose(Session session)
		{
		}

		private void OnOpen(Session session)
		{
			onNewSession.Call(session);
		}

		public void Broadcast(string message)
		{
			server.Broadcast(message);
		}

		public void Stop()
		{
			server.Stop();
		}

		public void Initialize() { }

		public void OnNewSession(Action<ISession> callback)
		{
			onNewSession += callback;
		}

		public void OnReceive(Action<ISession, string> callback)
		{
			onReceive += callback;
		}

		public void Start()
		{
			server.Start();
		}

		public void Send(ISession session, string message)
		{
			session.Send(message);
		}
	}
}