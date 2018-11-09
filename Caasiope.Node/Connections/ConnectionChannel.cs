using System;
using System.Text;
using Caasiope.Log;
using Caasiope.P2P;
using Helios.Common.Logs;
using Helios.JSON;

namespace Caasiope.Node.Connections
{
    public interface IConnectionChannel
    {
        void Broadcast(NotificationMessage message);
        void Send(IConnectionSession session, RequestMessage message);
        void Send(IConnectionSession session, NotificationMessage message);
        void Send(IConnectionSession session, ResponseMessage message);
        void SendError(IConnectionSession session, string crid);
        
        byte GetChannelByte();

        JsonMessageFactory Factory { get; }
        IDispatcher<IConnectionSession> Dispatcher { get; }
    }

    internal class ConnectionChannel : IConnectionChannel
    {
        private readonly IChannel Channel;
        private readonly Func<IConnectionSession, bool> shouldOpen;
        private readonly ILogger messageLogger;

        public ConnectionChannel(IChannel channel, JsonMessageFactory factory, IDispatcher<IConnectionSession> dispatcher, Func<IConnectionSession, bool> shouldOpen, ILogger messageLogger)
        {
            this.Factory = factory;
            this.Dispatcher = dispatcher;
            this.shouldOpen = shouldOpen;
            this.messageLogger = messageLogger;
            Channel = channel;
        }

        public void Send(IConnectionSession session, ResponseMessage message)
        {
            var responseMessage = Factory.SerializeResponse(message);
            messageLogger.LogDebug($"Sent: {responseMessage}");

            var bytes = Encoding.UTF8.GetBytes(responseMessage);
            Channel.Send(session.Session, bytes);
        }

        public void Send(IConnectionSession session, ErrorMessage message)
        {
            var responseMessage = Factory.SerializeError(message);
            messageLogger.LogDebug($"Sent: {responseMessage}");

            var bytes = Encoding.UTF8.GetBytes(responseMessage);
            Channel.Send(session.Session, bytes);
        }

        public void Broadcast(NotificationMessage notification)
        {
            var serialized = Factory.SerializeNotification(notification);
            messageLogger.LogDebug($"Broadcasted: {serialized}");

            var bytes = Encoding.UTF8.GetBytes(serialized);
            Channel.Broadcast(bytes);
        }

        public void Send(IConnectionSession session, RequestMessage message)
        {
            var serialized = Factory.SerializeRequest(message);
            messageLogger.LogDebug($"Sent: {serialized}");

            var bytes = Encoding.UTF8.GetBytes(serialized);
            Channel.Send(session.Session, bytes);
        }

        public void Send(IConnectionSession session, NotificationMessage message)
        {
            var serialized = Factory.SerializeNotification(message);
            messageLogger.LogDebug($"Sent: {serialized}");

            var bytes = Encoding.UTF8.GetBytes(serialized);
            Channel.Send(session.Session, bytes);
        }

        public void Send(IConnectionSession session, Request request)
        {
            Send(session, new RequestMessage(request, request.GetType().Name, Guid.NewGuid().ToString("N")));
        }

        public void SendError(IConnectionSession session, string crid)
        {
            Send(session, new ErrorMessage(crid, (byte)ResultCode.Failed));
        }

        public byte GetChannelByte()
        {
            return Channel.GetChannelByte();
        }

        public JsonMessageFactory Factory { get; }

        public IDispatcher<IConnectionSession> Dispatcher { get; }

        public void OnReceived(Action<ISession, byte[]> callback)
        {
            Channel.OnReceived(callback);
        }

        public void ShouldOpen(IConnectionSession session)
        {
            if(shouldOpen(session))
                session.Session.OpenChannel(Channel);
        }
    }
}