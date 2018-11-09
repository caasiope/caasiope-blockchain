using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helios.WebSocket.NetworkConnection
{
    public class ConnectionStateMachine
    {
        private const byte ReconnectionDelay = 7;
        private const byte AttemptsToConnect = 10;

        private readonly Dictionary<Type, State> states;
        private State currentState;

        public Action TryConnect { get; }
        public Action TryDisconnect { get; }

        public ConnectionStateMachine(Action tryConnect, Action tryDisconnect)
        {
            TryConnect = tryConnect;
            TryDisconnect = tryDisconnect;

            states = GetStates();
            currentState = states[typeof(DisconnectedState)];
        }

        private Dictionary<Type, State> GetStates()
        {
            return new Dictionary<Type, State>
            {
                {typeof (DisconnectedState), new DisconnectedState(ChangeStateInternal, TryConnect, TryDisconnect)},
                {typeof (ConnectingState), new ConnectingState(ChangeStateInternal, ReconnectionDelay, AttemptsToConnect, TryConnect, TryDisconnect)},
                {typeof (ConnectedState), new ConnectedState(ChangeStateInternal)}
            };
        }

        private void ChangeStateInternal(Type current, Type nextState)
        {
            Debug.Assert(currentState.GetType() == current);
            Debug.Assert(currentState.GetType() != nextState);

            currentState = states[nextState];
            currentState.Enter();
        }

        public void Connect()
        {
            if (currentState is ConnectingState)
                return;

            currentState.Exit();
            ChangeStateInternal(currentState.GetType(), typeof(ConnectingState));
        }

        public void Disconnect()
        {
            if (currentState is DisconnectedState)
                return;

            currentState.Exit();
            ChangeStateInternal(currentState.GetType(), typeof(DisconnectedState));
        }

        public void Pulse()
        {
            currentState.OnPulse();
        }

        public void OnConnected()
        {
            currentState.OnConnected();
        }

        public void OnDisconnected()
        {
            currentState.OnDisconnected();
        }
    }
}