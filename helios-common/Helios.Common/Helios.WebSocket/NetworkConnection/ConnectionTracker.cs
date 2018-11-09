using System;
using System.Threading;
using Helios.Common.Synchronization;

namespace Helios.WebSocket.NetworkConnection
{
    public interface IConnectionTracker
    {
        void Connect();
        void Disconnect();
        void OnConnected();
        void OnDisconnected();
        void Pulse();
        bool IsConnected { get; }
    }

    public class ConnectionTracker : IConnectionTracker
    {
        private readonly MonitorLocker locker = new MonitorLocker();
        private const int PulseInterval = 1000;
        private readonly Timer pulseTimer;
        private readonly ConnectionStateMachine stateMachine;
        private volatile bool isConnected; // OLOLO volatile looks odd

        public ConnectionTracker(Action tryConnect, Action tryDisconnect)
        {
            stateMachine = new ConnectionStateMachine(tryConnect, tryDisconnect);

            pulseTimer = new Timer(o => ((IConnectionTracker)this).Pulse(), null, Timeout.Infinite, Timeout.Infinite);
        }

        void IConnectionTracker.Connect()
        {
            stateMachine.Connect();
            pulseTimer.Change(0, Timeout.Infinite);
        }

        void IConnectionTracker.Disconnect()
        {
            stateMachine.Disconnect();
            pulseTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        void IConnectionTracker.OnConnected()
        {
            isConnected = true;
            using (locker.CreateLock())
            {
                stateMachine.OnConnected();
            }
        }

        void IConnectionTracker.OnDisconnected()
        {
            isConnected = false;
            using (locker.CreateLock())
            {
                stateMachine.OnDisconnected();
            }
        }

        bool IConnectionTracker.IsConnected => isConnected;

        void IConnectionTracker.Pulse()
        {
            using (locker.CreateLock())
            {
                stateMachine.Pulse();
                pulseTimer.Change(PulseInterval, Timeout.Infinite);
            }
        }
    }
}