using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Helios.Common.Synchronization;

namespace Caasiope.P2P
{
    internal class ThreadPool
    {
        private class DedicatedThread
        {
            private readonly ThreadPool pool;
            private readonly Thread thread;
            private bool isTerminated = false;
            private Action action;
            private readonly AutoResetEvent trigger = new AutoResetEvent(false);

            public DedicatedThread(ThreadPool pool)
            {
                this.pool = pool;
                thread = new Thread(ThreadLoop) { IsBackground = true };
                thread.Start();
            }

            public string Name
            {
                get => thread.Name;
                set => thread.Name = value;
            }

            private void ThreadLoop()
            {
                while (!isTerminated)
                {
                    try
                    {
                        trigger.WaitOne();
                        action();
                    }
                    catch (Exception e)
                    {
                        // TODO log
                    }
                    finally
                    {
                        action = null;
                        pool.Idle(this);
                    }
                }
            }

            public void Do(Action action)
            {
                Debug.Assert(action != null);
                Debug.Assert(this.action == null);
                this.action = action;
                trigger.Set();
            }
        }

        private readonly int MAX;
        private int count;

        // private readonly DedicatedThread[] threads;
        private readonly MonitorLocker locker = new MonitorLocker();
        private readonly Queue<DedicatedThread> idles = new Queue<DedicatedThread>();

        public ThreadPool(int max)
        {
            MAX = max;
        }

        public bool TryRun(Action action)
        {
            DedicatedThread thread;
            using (locker.CreateLock())
            {
                if (!TryGetThread(out thread))
                    return false;
            }
            thread.Do(action);
            return true;
        }

        private bool TryGetThread(out DedicatedThread thread)
        {
            // get idle
            if (idles.Count > 0)
            {
                thread = idles.Dequeue();
                return true;
            }

            // create new thread
            if (count < MAX)
            {
                thread = new DedicatedThread(this);
                count++;
                thread.Name = $"Network Thread {count}";
                return true;
            }

            thread = null;
            return false;
        }

        private void Idle(DedicatedThread thread)
        {
            using (locker.CreateLock())
            {
                idles.Enqueue(thread);
            }
        }
    }
}