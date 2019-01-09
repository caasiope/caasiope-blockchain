using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.JSON.API.Notifications;
using Caasiope.Explorer.Types;
using Caasiope.Protocol.Types;
using Helios.Common.Synchronization;
using Helios.JSON;

namespace Caasiope.Explorer.Managers.NotificationManagers
{
    public class LedgerNotificationManager : INotificationManager
    {
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly HashSet<ISession> subscriptors = new HashSet<ISession>();

        public Action<ISession, NotificationMessage> Send { get; set; }

        public void ListenTo(ISession session, Topic topic)
        {
            if (!(topic is LedgerTopic))
                return;

            using (locker.CreateLock())
            {
                subscriptors.Add(session);
            }
        }

        public void Notify(SignedLedger ledger)
        {
            foreach (var subscriptor in subscriptors)
            {
                var notification = new LedgerNotification()
                {
                    Hash = ledger.Hash.ToBase64(),
                    Height = ledger.GetHeight(),
                    Timestamp = ledger.GetTimestamp(),
                    Transactions = ledger.Ledger.Block.Transactions.Count()
                };
                Send(subscriptor, new NotificationMessage(notification));
            }
        }
    }
}