using System;
using System.Net;
using Helios.Common.Extensions;
using Helios.Common.Logs;
using Helios.Common.Synchronization;

namespace Caasiope.P2P
{
    // this is made to associate each connection to a node
    internal class NodeSession
    {
        private static readonly ThreadPool ThreadPool = new ThreadPool(100);

        // is it good to put the sending queue here and not in the peer session
        private readonly SynchronizedQueue<byte[]> messages = new SynchronizedQueue<byte[]>();
        
        private readonly TcpSession session;
        private readonly ILogger logger;
        private readonly NodeConnection connection;
        private ISessionListener listener;

        public IPEndPoint LocalEndPoint => session.LocalEndPoint;
        public IPEndPoint RemoteEndPoint => session.RemoteEndPoint;
        public Persona Persona => session.Persona;

        // TODO use observer pattern
        public Action<IPEndPoint, IPEndPoint> OnAuthenticated { get; set; }
        public Action<DisconnectReason> OnDisconnected { get; set; }

        public NodeSession(TcpSession session, ILogger logger)
        {
            connection = new NodeConnection();
            connection.StateChanged += (old, current) =>
            {
                // Console.WriteLine($"{RemoteEndPoint} Connection State {old} -> {current}");

                // ugly
                if (current == ConnectionState.Disconnecting)
                {
                    listener?.OnClose();
                    listener = null;
                    messages.CancelWaiting();
                    session.Close();

                    connection.State = ConnectionState.Disconnected;

                    OnDisconnected.Call(connection.Reason);
                }
            };
            this.session = session;
            this.logger = logger;
        }

        public bool Run()
        {
            if (!ThreadPool.TryRun(ClientRead))
            {
                Disconnect(DisconnectReason.NoThreadAvailable);
                return false;
            }

            return true;
        }

        private void ClientRead()
        {
            try
            {
                // TODO call authentication before calling the loop
                if (!session.Authenticate())
                {
                    Disconnect(DisconnectReason.AuthenticationFailed);
                    return;
                }

                connection.State = ConnectionState.Authenticated;

                if (!ThreadPool.TryRun(SendLoop))
                {
                    Disconnect(DisconnectReason.NoThreadAvailable);
                    return;
                }
            }
            catch (Exception e)
            {
                // Console.WriteLine(e);

                Disconnect(DisconnectReason.InitializationFailed);
                return;
            }

            ClientReadLoop();
        }
        
        // method for the write thread
        private void SendLoop()
        {
            while (true)
            {
                try
                {
                    if (!messages.Dequeue(out var message))
                        break; // it was cancelled

                    if(!session.Write(message[0], message.SubArray(1, message.Length - 1)))
                        Disconnect(DisconnectReason.ErrorWhenWrite);
                }
                catch (Exception e)
                {
                    // Console.WriteLine(e);
                    Disconnect(DisconnectReason.ErrorInSendLoop);
                }
            }
        }

        // method for the read thread
        // TODO why client ?
        private void ClientReadLoop()
        {
            try
            {
                OnAuthenticated.Call(session.LocalEndPoint, session.RemoteEndPoint);

                connection.State = ConnectionState.Connected;

                byte channel = 255;
                var buffer = new byte[0];
                while (session.Read(ref channel, ref buffer)) // The plan is that after session.Dispose throws an exception
                {
                    listener?.OnMessage(channel, buffer); // TODO send also client
                }
            }
            catch (Exception e)
            {
                logger.Log($"[{RemoteEndPoint}] Disconnecting, error in ClientReadLoop {e.Message}");

                Disconnect(DisconnectReason.ErrorInReadLoop);
            }
        }

        public void Send(byte channel, byte[] data)
        {
            var buffer = new byte[data.Length + 1];
            buffer[0] = channel;
            data.CopyTo(buffer, 1);

            messages.Enqueue(buffer);
        }

        public void Connect(ISessionListener listener)
        {
            this.listener = listener;
        }

        public void Disconnect(DisconnectReason reason)
        {
            connection.Disconnect(reason);
        }

        public ConnectionState GetConnectionState() => connection.State;

        public TimeSpan GetPing()
        {
            return connection.GetPing();
        }

        public DateTime GetLastPong()
        {
            return connection.GetLastPong();
        }

        public bool OnPing()
        {
            return connection.OnPing();
        }

        public bool OnPong()
        {
            return connection.OnPong();
        }
    }
}