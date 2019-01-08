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
        [Injected] public ILedgerService LedgerService;

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
                subscriptors.Add(session);

                SendNotification(session, LedgerService.LedgerManager.LastLedger.GetHeight());
            }
        }

        public void Notify(SignedLedger ledger)
        {
            foreach (var subscriptor in subscriptors)
            {
                if (!TryUpdateFunds())
                    continue;

                SendNotification(subscriptor, ledger.Ledger.LedgerLight.Height);
            }
        }

        private void SendNotification(ISession subscriptor, long height)
        {
            var changes = funds.ToDictionary(_ => _.Key, __ => -Amount.ToWholeDecimal(__.Value));

            var notification = new FundsNotification()
            {
                Funds = changes,
                Height = height
            };
            Send(subscriptor, new NotificationMessage(notification));
        }

        private bool TryUpdateFunds()
        {
            var isUpdated = false;
            foreach (var issuer in issuers)
            {
                if (!LedgerService.LedgerManager.LedgerState.TryGetAccount(issuer.Address, out var account))
                    continue;

                var newBalance = account.GetBalance(issuer.Currency);
                var currency = Currency.ToSymbol(issuer.Currency);

                if (funds[currency] != newBalance)
                {
                    isUpdated = true;
                    funds[currency] = newBalance;
                }
            }
            return isUpdated;
        }

        public void Initialize()
        {
            Injector.Inject(this);

            foreach (var issuer in LiveService.IssuerManager.GetIssuers())
            {
                issuers.Add(issuer);

                if (LedgerService.LedgerManager.LedgerState.TryGetAccount(issuer.Address, out var account))
                    funds.Add(Currency.ToSymbol(issuer.Currency), account.GetBalance(issuer.Currency));
            }
        }
    }
}