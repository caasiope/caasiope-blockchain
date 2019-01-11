using System;
using System.Collections.Generic;
using Caasiope.Explorer.Managers.NotificationManagers;
using Caasiope.Explorer.Types;
using Caasiope.Protocol.Types;
using Helios.Common.Logs;
using Helios.JSON;

namespace Caasiope.Explorer.Managers
{
    // subscription manager ?
    public class SubscriptionManager
    {
        public OrderBookNotificationManager OrderBookNotificationManager { get; }

        private readonly List<INotificationManager> managers = new List<INotificationManager>();
        private readonly FundsNotificationManager fundsNotificationManager;
        private readonly AddressNotificationManager addressNotificationManager;
        private Action<ISession, NotificationMessage> send;
        private readonly ILogger logger;

        public SubscriptionManager(ILogger logger)
        {
            this.logger = logger;
            AddManager(new TransactionNotificationManager());
            AddManager(new LedgerNotificationManager());
            addressNotificationManager = AddManager(new AddressNotificationManager());
            OrderBookNotificationManager = AddManager(new OrderBookNotificationManager());
            fundsNotificationManager = AddManager(new FundsNotificationManager());
        }

        public void ListenTo(ISession session, Topic topic)
        {
            // TODO may be use switch?
            // TODO move lock here?
            managers.ForEach(_ => _.ListenTo(session, topic));
        }

        public void Notify(SignedLedger ledger)
        {
            foreach (var manager in managers)
            {
                try
                {
                    manager.Notify(ledger);
                }
                catch (Exception e)
                {
                    logger.Log("SubscriptionManager", e);
                }
            }
        }

        private T AddManager<T>(T manager) where T : INotificationManager
        {
            managers.Add(manager);
            return manager;
        }

        public void Initialize()
        {
            managers.ForEach(_ => _.Send += send);
            fundsNotificationManager.Initialize();
            addressNotificationManager.Initialize();
        }

        public void OnSend(Action<ISession, NotificationMessage> callback)
        {
            send += callback;
        }

        public void OnClose(ISession session)
        {
            managers.ForEach(_ => _.OnClose(session));
        }
    }

    public interface INotificationManager
    {
        void ListenTo(ISession session, Topic topic);
        void Notify(SignedLedger ledger);
        Action<ISession, NotificationMessage> Send { get; set; }
        void OnClose(ISession session);
    }
}
