using System;
using Caasiope.Explorer.JSON.API;
using Caasiope.Explorer.Managers;
using Caasiope.Log;
using Caasiope.Node;
using Caasiope.Node.Connections;
using Caasiope.Node.Services;
using Helios.Common.Concepts.Services;
using Helios.Common.Logs;
using Helios.JSON;

namespace Caasiope.Explorer
{
    public interface IExplorerConnectionService : IWebSocketServerService
    {
        NotificationManager NotificationManager { get; }
    }

	public class ExplorerConnectionService : WebSocketServerService, IExplorerConnectionService
	{
        public NotificationManager NotificationManager { get; } = new NotificationManager();
        public ExplorerConnectionService(WebSocketServer server) : base(server, new BlockchainExplorerApi().JsonMessageFactory) { }
	}

    public interface IWebSocketServerService : IService
    {
        void Send(ISession session, NotificationMessage message);
    }

    public class WebSocketServerService : Service
	{
		private readonly WebSocketServer server;
		private IDispatcher<ISession> dispatcher;
		private readonly JsonMessageFactory factory;
		
		[Injected] public ILiveService LiveService;

		public WebSocketServerService(WebSocketServer server, JsonMessageFactory factory)
		{
		    Logger = new LoggerAdapter(Name);
		    this.server = server;
		    this.factory = factory;
		}

		public void SetDispatcher(IDispatcher<ISession> dispatcher)
		{
		    this.dispatcher = dispatcher;
        }

        protected override void OnInitialize()
		{
            server.OnReceive(OnReceive);

            Injector.Inject(this);
			Injector.Inject(dispatcher);
			server.Initialize();
        }

		public void OnReceive(ISession session, string message)
		{
			string crid = null;
			try
			{
				var isError = false;
				var wrapper = factory.DeserializeMessage(message, out crid, ref isError);

				if (isError)
					return;

				if(!dispatcher.Dispatch(session, wrapper, (r, rc) => Send(session, new ResponseMessage(r, r.GetType().Name, crid, (byte)rc))))
				    SendError(session, crid);
			}
			catch(Exception e)
			{
				SendError(session, crid);
                Logger.Log("ConnectionService ex:", e);
            }
		}

		public void Send(ISession session, ResponseMessage message)
		{
			var responseMessage = factory.SerializeResponse(message);
			server.Send(session, responseMessage);
		}

		public void Send(ISession session, ErrorMessage message)
		{
			var responseMessage = factory.SerializeError(message);
			server.Send(session, responseMessage);
		}

		public void Broadcast(NotificationMessage notification)
		{
			var serialized = factory.SerializeNotification(notification);
			server.Broadcast(serialized);
		}

		public void Send(ISession session, RequestMessage message)
		{
			var serialized = factory.SerializeRequest(message);
			server.Send(session, serialized);
		}

		public void Send(ISession session, NotificationMessage message)
		{
			var serialized = factory.SerializeNotification(message);
			server.Send(session, serialized);
		}

		public void Send(ISession session, Request request)
		{
			Send(session, new RequestMessage(request, request.GetType().Name, Guid.NewGuid().ToString("N")));
		}

		private void SendError(ISession session, string crid)
		{
			Send(session, new ErrorMessage(crid, (byte)Helios.JSON.ResultCode.Failed));
		}

		protected override void OnStart()
		{
			server.Start();
		}

		protected override void OnStop()
		{
			server.Stop();
		}

		public override ILogger Logger { get; }
	}
}