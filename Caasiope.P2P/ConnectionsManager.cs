using Helios.Common;
using Helios.Common.Synchronization;

namespace Caasiope.P2P
{
    public class ConnectionsManager
    {
        private readonly MonitorLocker locker = new MonitorLocker();
        private readonly int min;
        private readonly int max;
        private uint connected;

        public ConnectionsManager(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public bool IsRequireMorePeers()
        {
            using (locker.CreateLock())
            {
                return connected < min;
            }
        }

        public bool IsReachedMaxPeerLimit()
        {
            using (locker.CreateLock())
            {
                return connected >= max;
            }
        }

        public void PeerDisconnected()
        {
            using (locker.CreateLock())
            {
                if (connected == 0)
                    return;
                connected--;
            }
        }

        internal void NewConnection()
        {
            using (locker.CreateLock())
            {
                connected++;
            }
        }
    }
}