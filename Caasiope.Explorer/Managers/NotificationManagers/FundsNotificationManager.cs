using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly HashSet<ISession> subscriptors = new HashSet<ISession>();
        private readonly Dictionary<string, long> funds = new Dictionary<string, long>();
        private readonly List<Issuer> issuers = new List<Issuer>();

        public Action<ISession, NotificationMessage> Send { get; set; }

        public void ListenTo(ISession session, Topic topic)
        {
            if (!(topic is FundsTopic))
                return;

            using (locker.CreateLock())
            {
                if(!subscriptors.Add(session))
                    return;

                SendNotification(session, funds.ToDictionary(_ => _.Key, __ => -Amount.ToWholeDecimal(__.Value)));
            }
        }

        public void Notify(SignedLedger ledger)
        {
            foreach (var subscriptor in subscriptors)
            {
                SendNotification(subscriptor, GetChanges());
            }
        }

        private void SendNotification(ISession subscriptor, Dictionary<string, decimal> changes)
        {
            var notification = new FundsNotification()
            {
                Funds = changes
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

                if (funds[currency] != newBalance)
                {
                    funds[currency] = newBalance;
                    results.Add(currency, -Amount.ToWholeDecimal(newBalance));
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