using System;
using System.Collections.Generic;
using Caasiope.Explorer.Types;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;
using Helios.JSON;

namespace Caasiope.Explorer.Managers.NotificationManagers
{
    public class OrderBookNotificationManager : INotificationManager
    {
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly Dictionary<ISession, HashSet<string>> subscriptors = new Dictionary<ISession, HashSet<string>>();

        public Action<ISession, NotificationMessage> Send { get; set; }

        public void ListenTo(ISession session, Topic topic)
        {
            if (!(topic is OrderBookTopic addressFilter))
                return;

            var symbol = addressFilter.Symbol;

            using (locker.CreateLock())
            {
                subscriptors.GetOrCreate(session).Add(symbol);
            }

            throw new NotImplementedException();
        }

        public void Notify(SignedLedger ledger)
        {
            // TODO 
        }

    }
}