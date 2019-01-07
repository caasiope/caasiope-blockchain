using System;
using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Notifications;
using Caasiope.Explorer.Types;
using Caasiope.Node;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Synchronization;
using Helios.JSON;

namespace Caasiope.Explorer.Managers.NotificationManagers
{
    public class FundsNotificationManager : INotificationManager
    {
        [Injected] public ILiveService LiveService;

        private readonly MonitorLocker locker = new MonitorLocker();
        private readonly MonitorLocker fundsLocker = new MonitorLocker();

        private readonly HashSet<ISession> subscriptors = new HashSet<ISession>();
        private readonly Dictionary<string, decimal> funds = new Dictionary<string, decimal>();
        private readonly List<Issuer> issuers = new List<Issuer>();

        public Action<ISession, NotificationMessage> Send { get; set; }

        public void ListenTo(ISession session, Topic topic)
        {
            if (!(topic is LedgerTopic))
                return;

            using (locker.CreateLock())
            {
                subscriptors.Add(session);
            }

            SendNotification(session);
        }

        public void Notify(SignedLedger ledger)
        {
            foreach (var subscriptor in subscriptors)
            {
                SendNotification(subscriptor);
            }
        }

        private void SendNotification(ISession subscriptor)
        {
            var notification = new FundsNotification()
            {
                Funds = GetChanges()
            };
            Send(subscriptor, new NotificationMessage(notification));
        }

        private Dictionary<string, decimal> GetChanges()
        {
            var results = new Dictionary<string, decimal>();
            foreach (var issuer in issuers)
            {
                if (!LiveService.AccountManager.TryGetAccount(issuer.Address, out var account))
                    continue;

                var newBalance = account.Account.GetBalance(issuer.Currency);
                var currency = Currency.ToSymbol(issuer.Currency);

                // TODO Looks weird
                using (fundsLocker.CreateLock())
                {
                    if (funds[currency] != newBalance)
                    {
                        funds[currency] = newBalance;
                        results.Add(currency, Amount.ToWholeDecimal(-newBalance));
                    }
                }
            }

            return results;
        }

        public void Initialize()
        {
            Injector.Inject(this);

            foreach (var issuer in LiveService.IssuerManager.GetIssuers())
            {
                issuers.Add(issuer);

                if (LiveService.AccountManager.TryGetAccount(issuer.Address, out var account))
                    funds.Add(Currency.ToSymbol(issuer.Currency), account.Account.GetBalance(issuer.Currency));
            }
        }
    }
}