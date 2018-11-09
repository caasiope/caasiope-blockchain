using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Helios.Common.Extensions;

namespace Helios.Common.Synchronization
{
    public class SynchronizedBlockingState<T>
    {
        public class Listener
        {
            private readonly SynchronizedBlockingState<T> synchronizer;
            internal ManualResetEventSlim Acknowledgement = new ManualResetEventSlim();
            internal ManualResetEventSlim Resume = new ManualResetEventSlim();
            internal ManualResetEventSlim StatusProtector = new ManualResetEventSlim(true);
            public T State { get; private set; }

            public Listener(SynchronizedBlockingState<T> synchronizer)
            {
                this.synchronizer = synchronizer;
            }

            public WaitHandle CancelEvent { get { return synchronizer.CancelEvent; } }

            // waits until the state is set to target
            public void Wait(T target)
            {
                // we check if we are in the target state
                if (!synchronizer.hasChanged && State.Equals(target)) return;

                // wait for targeted state
                do { NextState(); } while (!State.Equals(target));
            }

            // wait for state to change
            public void NextState()
            {
                // signal that we are waiting for the next state
                Acknowledgement.Set();
                // wait until next state is set
                Resume.Wait();
                Resume.Reset();
                State = synchronizer.State;
                // now you can update status
                StatusProtector.Set();
            }
        }

        private readonly List<Listener> listeners = new List<Listener>();

        public T State { get; private set; }
        private bool hasChanged;
        public readonly ManualResetEvent CancelEvent = new ManualResetEvent(false);

        public Action<T> OnSet;

        private readonly object locker = new object();
        private bool isSynchronizing;
        public bool IsSynchronizing { get { lock (locker) { return isSynchronizing;} } set { lock (locker) { isSynchronizing = value; } } }

        public Listener CreateListener()
        {
            lock (locker)
            {
                // TODO handle the case when we add a listener while we wait
                Listener listener = new Listener(this);
                listeners.Add(listener);
                return listener;
            }
        }

        public bool SetAndWait(T status)
        {
            if (!Set(status))
                return false;
            if(!WaitAcknowledgement())
                throw new AssertionFailedException("if we can set then we should be able to wait !");
            return true;
        }

        private bool SetSynchronizing()
        {
            lock (locker)
            {
                if (isSynchronizing)
                    return false;
                isSynchronizing = true;
                return true;
            }
        }

        // set state and start synchronizing if not yet synchronizing
        public bool Set(T status)
        {
            if (!SetSynchronizing())
                return false;
            Set_Unsafe(status);
            return true;
        }
        
        private void Set_Unsafe(T status)
        {
            CancelEvent.Set();
            OnSet.Call(status);
            if (listeners.Count > 0) WaitHandle.WaitAll(listeners.Select(o => o.StatusProtector.WaitHandle).ToArray());
            // set new state
            State = status;
            hasChanged = true;
        }

        // wait acknowledgement if we are synchronizing
        public bool WaitAcknowledgement()
        {
            if (!IsSynchronizing)
                return false;
            WaitAcknowledgement_Unsafe();
            IsSynchronizing = false;
            return true;
        }

        private void WaitAcknowledgement_Unsafe()
        {
            // wait for listeners to acknowledge
            if (listeners.Count > 0) WaitHandle.WaitAll(listeners.Select(o => o.Acknowledgement.WaitHandle).ToArray());
            CancelEvent.Reset();

            hasChanged = false;

            foreach (var listener in listeners)
            {
                // cause the listening threads to run again
                listener.StatusProtector.Reset();
                // dont update status until I am finished
                listener.Acknowledgement.Reset();
                listener.Resume.Set();
            }
        }
    }
}