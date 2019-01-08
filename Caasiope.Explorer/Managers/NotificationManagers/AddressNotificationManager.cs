using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.JSON.API;
using Caasiope.Explorer.JSON.API.Notifications;
using Caasiope.Explorer.Types;
using Caasiope.Node;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;
using Helios.JSON;

namespace Caasiope.Explorer.Managers.NotificationManagers
{
    public class AddressNotificationManager : INotificationManager
    {
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly Dictionary<ISession, AddressSubscriptor> subscriptors = new Dictionary<ISession, AddressSubscriptor>();

        public Action<ISession, NotificationMessage> Send { get; set; }

        public void ListenTo(ISession session, Topic topic)
        {
            if (!(topic is AddressTopic addressFilter))
                return;

            var address = addressFilter.Address;

            using (locker.CreateLock())
            {
                subscriptors.GetOrCreate(session).Subscribe(address);
            }
        }

        public void Notify(SignedLedger ledger)
        {
            foreach (var subscriptor in subscriptors)
            {
                foreach (var notification in GetNotifications(subscriptor.Value, ledger))
                {
                    Send(subscriptor.Key, new NotificationMessage(notification));
                }
            }
        }

        private List<AddressNotification> GetNotifications(AddressSubscriptor subscriptor, SignedLedger ledger)
        {
            var inputs = ledger.Ledger.Block.Transactions.SelectMany(_ => _.Transaction.Inputs.Select(__ => __.Address));
            var outputs = ledger.Ledger.Block.Transactions.SelectMany(_ => _.Transaction.Outputs.Select(__ => __.Address));
            var addresses = inputs.Union(outputs).ToList();
            return subscriptor.GetNotifications(addresses);
        }
    }

    internal class AddressSubscriptor
    {
        [Injected] public ILedgerService LedgerService;

        private readonly HashSet<Address> addresses = new HashSet<Address>();

        public void Subscribe(Address address)
        {
            addresses.Add(address);
        }

        public List<AddressNotification> GetNotifications(List<Address> toCheck)
        {
            var results = new List<AddressNotification>();

            foreach (var address in toCheck)
            {
                if(!addresses.Contains(address))
                    continue;

                if (LedgerService.LedgerManager.LedgerState.TryGetAccount(address, out var account))
                {
                    results.Add(new AddressNotification
                    {
                        Address = account.Address.Encoded,
                        Balance = account.Balances.ToDictionary(_ => Currency.ToSymbol(_.Currency), __ => Amount.ToWholeDecimal(__.Amount))
                    });
                }
            }
            return results;
        }
    }
}