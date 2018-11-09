using System;
using System.Diagnostics;
using Helios.Common.Extensions;

namespace Helios.WebSocket.NetworkConnection
{
    public abstract class State
    {
        protected Action<Type> OnChangeState;
        protected Action TryDisconnect;
        protected Action TryConnect;

        protected State(Action<Type, Type> onChangeState, Action tryConnect, Action tryDisconnect)
        {
            OnChangeState = next => { onChangeState(GetType(), next); };
            TryDisconnect = tryDisconnect;
            TryConnect = tryConnect;
        }

        public abstract void Enter();
        public void Exit() { BeforeExit(); }
        public virtual void OnConnected() { ExitInternal<ConnectedState>(); }
        public abstract void OnDisconnected();

        protected abstract void BeforeExit();

        protected void ExitInternal<T>()
        {
            BeforeExit();
            OnChangeState.Call(typeof(T));
        }

        public abstract void OnPulse();
    }


    public class DisconnectedState : State
    {
        private int pulsesLeft;
        public DisconnectedState(Action<Type, Type> onChangeState, Action tryConnect, Action tryDisconnect) : base(onChangeState, tryConnect, tryDisconnect) { }

        public override void Enter()
        {
            TryDisconnect.Call();
            pulsesLeft = 0;
        }
        protected void ReconnectionTick()
        {
            TryConnect.Call();
        }

        protected override void BeforeExit()
        {
            pulsesLeft = 0;
        }

        public override void OnPulse()
        {
            if (++pulsesLeft >= 10)
            {
                pulsesLeft = 0;
                ReconnectionTick();
            }
        }

        public override void OnDisconnected() { }
    }

    public class ConnectingState : State
    {
        private readonly byte reconnectionDelay;
        private readonly byte attemptsToConnect;
        private byte totalAttemptsCount;
        private byte pulsesLeft;

        public ConnectingState(Action<Type, Type> onChangeState, byte reconnectionDelay, byte attemptsToConnect, Action tryConnect, Action tryDisconnect) : base(onChangeState, tryConnect, tryDisconnect)
        {
            Debug.Assert(reconnectionDelay != 0);
            Debug.Assert(attemptsToConnect != 0);

            this.reconnectionDelay = reconnectionDelay;
            this.attemptsToConnect = attemptsToConnect;
            this.pulsesLeft = reconnectionDelay; // makes it connect immediately on startup
        }

        protected void ReconnectionTick()
        {
            if (++totalAttemptsCount <= attemptsToConnect)
                TryConnect.Call();
            else
                ExitInternal<DisconnectedState>();
        }

        public override void OnPulse()
        {
            if (++pulsesLeft >= reconnectionDelay)
            {
                pulsesLeft = 0;
                ReconnectionTick();
            }
        }

        public override void Enter()
        {
            totalAttemptsCount = 0;
        }

        protected override void BeforeExit()
        {
            pulsesLeft = totalAttemptsCount = 0;
        }

        public override void OnDisconnected() { }
    }

    public class ConnectedState : State
    {
        public ConnectedState(Action<Type, Type> onChangeState) : base(onChangeState, null, null) { }

        public override void OnPulse() { }

        public override void Enter() { }

        public override void OnConnected() { }

        public override void OnDisconnected() { ExitInternal<ConnectingState>(); }

        protected override void BeforeExit() { }
    }
}