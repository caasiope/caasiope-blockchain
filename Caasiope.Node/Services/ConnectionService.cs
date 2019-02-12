using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Caasiope.JSON.Helpers;
using Caasiope.Log;
using Caasiope.Node.Connections;
using Caasiope.P2P;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Services;
using Helios.Common.Extensions;
using Helios.Common.Logs;
using Helios.JSON;

namespace Caasiope.Node.Services
{
    public interface IConnectionService : IService
    {
        IConnectionChannel CreateChannel(byte channel, IDispatcher<IConnectionSession> dispatcher, JsonMessageFactory factory, Func<IConnectionSession, bool> shouldOpen);
        IConnectionChannel BlockchainChannel { get; }
        X509Certificate2 GetCertificate();
        void OnSessionConnected(Action<IConnectionSession> callback);
        IEnumerable<IPeer> GetConnectedPeers();
        IEnumerable<P2P.Node> GetAllNodes();
        IEnumerable<IPEndPoint> GetSelfEndPoints();
        void WipeNodeList();
    }

    public class ConnectionService : Service, IConnectionService
    {
	    private readonly Connections.IP2PConnection connection;

        public const byte BLOCKCHAIN_CHANNEL = 1;
        
        private Action<IConnectionSession> onSessionConnected;

        [Injected] public ILedgerService LedgerService;

        private readonly List<IDispatcher<IConnectionSession>> dispatchers = new List<IDispatcher<IConnectionSession>>();

        // get one dispatcher per channel
        public ConnectionService(Connections.IP2PConnection connection, bool messageLoggerEnabled)
        {
            this.connection = connection;
            Logger = new LoggerAdapter(Name);
            MessageLogger = messageLoggerEnabled ? new LoggerAdapter("MessageLogger") : (ILogger) new FakeLogger();
        }

        private ILogger MessageLogger { get; }

        protected override void OnInitialize()
        {
	        Injector.Inject(this);
            foreach (var dispatcher in dispatchers)
            {
                Injector.Inject(dispatcher);
            }

            LedgerService.LedgerManager.SubscribeOnNewLedger(BroadcastNewLedger);
            connection.Initialize(Logger);
            connection.OnConnected(OnSessionConnected);
        }

        public void OnReceive(IConnectionChannel channel, IConnectionSession session, byte[] bytes)
        {
            var factory = channel.Factory;
            var dispatcher = channel.Dispatcher;
            string crid = null;
            try
            {
                var message = Encoding.UTF8.GetString(bytes);

                MessageLogger.LogDebug($"Received: {message}");

                var isError = false;
                var wrapper = factory.DeserializeMessage(message, out crid, ref isError);
                
                if (isError)
                    return;


                if (!dispatcher.Dispatch(session, wrapper, (r, rc) => channel.Send(session, new ResponseMessage(r, r.GetType().Name, crid, (byte)rc))))
                    channel.SendError(session, crid);
            }
            catch (Exception e)
            {
                channel.SendError(session, crid);
                Logger.Log("ConnectionService ex:", e);
            }
        }

        private void BroadcastNewLedger(SignedLedger signedLedger)
        {
            var message = NotificationHelper.CreateSignedNewLedgerNotification(signedLedger);
            // broadcast the hash of the new ledger with the signature.
            BlockchainChannel.Broadcast(message);
            Logger.Log("Broadcast Signed New Ledger");
        } 

        protected override void OnStart()
        {
            LedgerService.StartedHandle.WaitOne();

            connection.Start();
        }

        protected override void OnStop()
        {
            connection.Stop();
        }

        public override ILogger Logger { get; }

        // link channel to dispatcher
        private IConnectionChannel RegisterChannel(IChannel channel, IDispatcher<IConnectionSession> dispatcher, JsonMessageFactory factory, Func<IConnectionSession, bool> shouldOpen)
        {
            dispatchers.Add(dispatcher);
            var connectionChannel = new ConnectionChannel(channel, factory, dispatcher, shouldOpen, MessageLogger);
            connection.OnConnected(session => OnSessionConnected(session, connectionChannel));
            connectionChannel.OnReceived((session, bytes) => OnReceive(connectionChannel, new ConnectionSession(session), bytes));
            return connectionChannel;
        }

        private void OnSessionConnected(ISession session, ConnectionChannel channel)
        {
            var wrapper = new ConnectionSession(session);
            channel.ShouldOpen(wrapper);
        }

        private void OnSessionConnected(ISession session)
        {
            var wrapper = new ConnectionSession(session);
            onSessionConnected.Call(wrapper);
        }
        
        public void OnSessionConnected(Action<IConnectionSession> callback)
        {
            onSessionConnected += callback;
        }

        public IEnumerable<IPeer> GetConnectedPeers()
        {
            return connection.GetConnectedPeers();
        }

        public IEnumerable<P2P.Node> GetAllNodes()
        {
            return connection.GetAllNodes();
        }

        public IEnumerable<IPEndPoint> GetSelfEndPoints()
        {
            return connection.GetSelfEndPoints();
        }

        public void WipeNodeList()
        {
            connection.WipeNodeList();
        }

        public IConnectionChannel BlockchainChannel { get; private set; }

        public X509Certificate2 GetCertificate()
        {
            return connection.GetCertificate();
        }

        public void SetBlockchainChannel(IConnectionChannel blockchain)
        {
            if(BlockchainChannel != null)
                throw new Exception("Please call this only once !");
            if(blockchain.GetChannelByte() != BLOCKCHAIN_CHANNEL)
                throw new Exception("Please set the right channel");

            BlockchainChannel = blockchain;
        }

        public IConnectionChannel CreateChannel(byte channel, IDispatcher<IConnectionSession> dispatcher, JsonMessageFactory factory, Func<IConnectionSession, bool> shouldOpen)
        {
            return RegisterChannel(connection.CreateChannel(channel), dispatcher, factory, shouldOpen);
        }

        private class FakeLogger : ILogger
        {
            public void Log(string message, Exception exception = null) { }
            public void LogInfo(string message, Exception exception = null) { }
            public void LogDebug(string message, Exception exception = null) { }
        }
    }
}
