using System;
using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Notifications;
using Caasiope.Explorer.Services;
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
        }

        public void Notify(SignedLedger ledger) { }

        public void OnClose(ISession session)
        {
            using (locker.CreateLock())
            {
                subscriptors.Remove(session);
            }
        }

        public void Notify(string symbol, List<Order> orders)
        {
            using (locker.CreateLock())
            {
                foreach (var subscriptor in subscriptors)
                {
                    if(subscriptor.Value.Contains(symbol))
                    {
                        var notification = new OrderBookNotification()
                        {
                            Symbol = symbol,
                            Orders = OrderConverter.GetOrders(orders)
                        };
                        Send(subscriptor.Key, new NotificationMessage(notification));
                    }
                }
            }
        }

    }
}