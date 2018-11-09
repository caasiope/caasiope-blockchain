using System.Collections.Generic;
using System.Threading;

namespace Helios.Common.Synchronization
{
    public class SynchronizedQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        private  readonly MonitorLocker locker = new MonitorLocker();

        private readonly AutoResetEvent enqueue = new AutoResetEvent(false);
        private readonly AutoResetEvent cancel = new AutoResetEvent(false);

        public void Enqueue(T item)
        {
            using (locker.CreateLock())
            {
                queue.Enqueue(item);
                enqueue.Set();
            }
        }

        public bool Dequeue(out T item)
        {
            using (locker.CreateLock())
            {
                // we have an item
                if (queue.Count > 0)
                {
                    item = queue.Dequeue();
                    return true;
                }
            }

            // wait for item or cancellation
            while (true)
            {
                // we need to wait
                var index = WaitHandle.WaitAny(new WaitHandle[]{ enqueue, cancel});

                // the waiting was cancelled
                if (index == 1)
                {
                    item = default(T);
                    return false;
                }

                // an item was added
                using (locker.CreateLock())
                {
                    // we have an item
                    if (queue.Count > 0) // should not happened
                    {
                        item = queue.Dequeue();
                        return true;
                    }
                }
            }
        }

        // cancels current waiting process
        public void CancelWaiting()
        {
            cancel.Set();
        }
    }
}