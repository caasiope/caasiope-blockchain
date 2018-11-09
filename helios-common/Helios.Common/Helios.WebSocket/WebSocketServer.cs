using System;
using System.Security.Cryptography.X509Certificates;
using Helios.Common.Configurations;
using Helios.Common.Extensions;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Helios.WebSocket
{
	public class WebSocketServer : WebSocketSharp.Server.WebSocketServer
	{
		public struct Configuration
		{
			public int Port;
			public string Ip;
			public string TLS_CERT;
			public string TLS_PWD;
		}

		public class Session : WebSocketBehavior
		{
            public Action onOpen;
            public Action<string> onMessage;
            public Action onClose;
            public Action onError;

            protected override void OnOpen()
		    {
		        onOpen.Call();
		    }

		    protected override void OnMessage(MessageEventArgs e)
		    {
		        onMessage.Call(e.Data);
		    }

		    protected override void OnClose(CloseEventArgs e)
		    {
		        onClose.Call();
            }

		    protected override void OnError(ErrorEventArgs e)
		    {
		        onError.Call();
            }
		}

		private WebSocketServer(string url) : base(url) { }
		private WebSocketServer(int port) : base(port) { }

		public static WebSocketServer CreateNew(Configuration configuration, Func<Session> create = null)
		{
			WebSocketServer server;
			if (!String.IsNullOrEmpty(configuration.Ip))
			{
				server = new WebSocketServer(configuration.Ip + ":" + configuration.Port);
			}
			else
			{
				server = new WebSocketServer(configuration.Port);
			}
			if(!String.IsNullOrEmpty(configuration.TLS_CERT))
			{
				server.SslConfiguration.ServerCertificate = new X509Certificate2(configuration.TLS_CERT, configuration.TLS_PWD);
			};
			server.AddWebSocketService("/", create ?? CreateSession);
			return server;
		}

		public static Session CreateSession() { return new Session(); }

		public void Broadcast(string message)
		{
			WebSocketServices.Broadcast(message);
		}
	}

    public class WebSocketServerConfiguration
    {
        public static WebSocketServer.Configuration LoadConfiguration(string path)
        {
            var lines = new DictionaryConfiguration(path);

            return new WebSocketServer.Configuration
            {
                Ip = lines.GetValue("Ip"),
                Port = int.Parse(lines.GetValue("Port")),
                TLS_CERT = lines.GetValue("TLS_CERT"),
                TLS_PWD = lines.GetValue("TLS_PWD"),
            };
        }
    }
}
