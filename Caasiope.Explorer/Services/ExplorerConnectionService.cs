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

namespace Caasiope.Explorer.Services
{
    public interface IExplorerConnectionService : IWebSocketServerService
    {
        SubscriptionManager SubscriptionManager { get; }
    }

	public class ExplorerConnectionService : WebSocketServerService, IExplorerConnectionService
	{
        public SubscriptionManager SubscriptionManager { get; }

	    public ExplorerConnectionService(WebSocketServer server) : base(server, new BlockchainExplorerApi().JsonMessageFactory)
	    {
            SubscriptionManager = new SubscriptionManager(Logger);
        }

	    protected override void OnInitialize()
	    {
	        base.OnInitialize();
	        SubscriptionManager.OnSend(Send);
            // TODO there is a problem. It cannot run from LedgerService thread!
            LedgerService.LedgerManager.SubscribeOnNewLedger(SubscriptionManager.Notify);
	        OrderBookService.OnOrderBookUpdated(SubscriptionManager.OrderBookNotificationManager.Notify);
        }

	    protected override void OnClose(ISession session)
	    {
	        SubscriptionManager.OnClose(session);
        }

        protected override void OnStart()
	    {
	        base.OnStart();
	        SubscriptionManager.Initialize();
	    }
	}

    public interface IWebSocketServerService : IService
    {
        void Send(ISession session, NotificationMessage message);
    }

    public class WebSocketServerService : Service
	{
	    [Injected] public IExplorerDataTransformationService ExplorerDataTransformationService;
	    [Injected] public ILedgerService LedgerService;
	    [Injected] public IOrderBookService OrderBookService;

        private readonly WebSocketServer server;
		private IDispatcher<ISession> dispatcher;
		private readonly JsonMessageFactory factory;
		
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
            server.OnClose(OnClose);

            Injector.Inject(this);
			Injector.Inject(dispatcher);
			server.Initialize();
        }

	    protected virtual void OnClose(ISession obj) { }

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
		    ExplorerDataTransformationService.StartedHandle.WaitOne();
            ExplorerDataTransformationService.WaitTransformationCompleted();
		    LedgerService.StartedHandle.WaitOne();

            server.Start();
		}

		protected override void OnStop()
		{
			server.Stop();
		}

		public override ILogger Logger { get; }
	}
}