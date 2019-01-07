using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.JSON.API;
using Caasiope.Explorer.JSON.API.Notifications;
using Caasiope.Explorer.Types;
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
                var notification = new LedgerNotification()
                {
                    Hash = ledger.Hash.ToBase64(),
                    Height = ledger.GetHeight(),
                    Timestamp = ledger.GetTimestamp(),
                    Transactions = GetTransactions(subscriptor.Value, ledger)
                };
                Send(subscriptor.Key, new NotificationMessage(notification));
            }
        }

        private List<JSON.API.Internals.Transaction> GetTransactions(AddressSubscriptor subscriptor, SignedLedger ledger)
        {
            return ledger.Ledger.Block.Transactions
                .Where(transaction => subscriptor.IsSubscribed(transaction, true))
                .Select(signed => TransactionConverter.GetTransaction(signed.Transaction)).ToList();
        }
    }

    internal class AddressSubscriptor
    {
        private readonly HashSet<Address> addresses = new HashSet<Address>();

        public void Subscribe(Address address)
        {
            addresses.Add(address);
        }

        public bool IsSubscribed(SignedTransaction transaction, bool unsubcribe = false)
        {
            var inputs = transaction.Transaction.Inputs.Select(_ => _.Address);
            var outputs = transaction.Transaction.Outputs.Select(_ => _.Address);
            var toCheck = inputs.Union(outputs).ToList();

            foreach (var address in toCheck)
            {
                if (addresses.Contains(address))
                {
                    if (unsubcribe)
                        addresses.Remove(address);
                    return true;
                }
            }

            return false;
        }
    }
}