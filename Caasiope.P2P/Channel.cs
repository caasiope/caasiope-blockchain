using System;
using Helios.Common.Extensions;

namespace Caasiope.P2P
{
    public interface IChannel
    {
        void Broadcast(byte[] data);
        void Send(ISession peer, byte[] data);
        void OnReceived(Action<ISession, byte[]> callback);

        byte GetChannelByte();
        void OnReceived(ISession session, byte[] message);
        int GetIndex();
    }

    public interface IChannelConnection
    {
        void Broadcast(IChannel channel, byte[] data);
        void Send(ISession session, IChannel channel, byte[] data);
    }

    public class Channel : IChannel
    {
        private readonly byte channel;
        private readonly IChannelConnection connection;
        private Action<ISession, byte[]> onReceived;
        private readonly int index;

        public Channel(byte channel, IChannelConnection connection, int index)
        {
            this.channel = channel;
            this.connection = connection;
            this.index = index;
        }

        public void Broadcast(byte[] data)
        {
            connection.Broadcast(this, data);
        }

        public void Send(ISession peer, byte[] data)
        {
            connection.Send(peer, this, data);
        }

        public void OnReceived(Action<ISession, byte[]> callback)
        {
            onReceived += callback;
        }

        public void OnReceived(ISession session, byte[] message)
        {
            onReceived.Call(session, message);
        }

        public int GetIndex()
        {
            return index;
        }

        public byte GetChannelByte()
        {
            return channel;
        }
    }
}