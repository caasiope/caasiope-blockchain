using System;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;

namespace Caasiope.P2P
{
    internal class NodeConnection
    {
        private class PingPong
        {
            private readonly DateTime ping;
            private DateTime pong;
            public static readonly PingPong DEFAULT = new PingPong(DateTime.MinValue, DateTime.MinValue);

            private PingPong(DateTime date)
            {
                ping = date;
            }

            private PingPong(DateTime sent, DateTime received)
            {
                ping = sent;
                pong = received;
            }

            public static PingPong Ping()
            {
                return new PingPong(DateTime.Now);
            }

            public void Pong()
            {
                pong = DateTime.Now;
            }

            public TimeSpan GetPing()
            {
                return pong - ping;
            }

            public DateTime GetLastPong()
            {
                return pong;
            }
        }

        private ConnectionState state = ConnectionState.Connecting;
        public Action<ConnectionState, ConnectionState> StateChanged { get; set; }
        
        private PingPong last = PingPong.DEFAULT;
        private PingPong current;

        private readonly MonitorLocker locker = new MonitorLocker();

        public bool OnPing()
        {
            using (locker.CreateLock())
            {
                if (current != null)
                    return false;
                current = PingPong.Ping();
                return true;
            }
        }

        public bool OnPong()
        {
            using (locker.CreateLock())
            {
                if (current == null)
                    return false;
                current.Pong();
                last = current;
                current = null;
                return true;
            }
        }

        public TimeSpan GetPing()
        {
            return last.GetPing();
        }

        public DateTime GetLastPong()
        {
            return last.GetLastPong();
        }

        public ConnectionState State
        {
            get
            {
                using (locker.CreateLock())
                {
                    return state;
                }
            }
            set
            {
                ConnectionState old;
                using (locker.CreateLock())
                {
                    // we cant return to a previous state
                    if (value <= state)
                        return;
                    old = state;
                    state = value;
                }
                StateChanged.Call(old, state);
            }
        }

        public void Disconnect(DisconnectReason reason)
        {
            Reason = reason;
            State = ConnectionState.Disconnecting;
        }

        public DisconnectReason Reason { get; private set; }
    }
}