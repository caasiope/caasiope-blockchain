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

namespace Caasiope.Explorer.Managers
{
    // subscription manager ?
    public class SubscriptionManager
    {
        private readonly List<INotificationManager> managers = new List<INotificationManager>();

        public Action<ISession, NotificationMessage> Send;

        public void ListenTo(ISession session, List<Topic> topics)
        {
            foreach (var topic in topics)
            {
                // TODO may be use switch?
                // TODO move lock here
                managers.ForEach(_ => _.ListenTo(session, topic));
            }
        }

        public void Notify(SignedLedger ledger)
        {
            foreach (var manager in managers)
            {
                manager.Notify(ledger);
            }
        }

        public SubscriptionManager()
        {
            managers.Add(new TransactionNotificationManager());
            managers.Add(new LedgerNotificationManager());
            managers.Add(new AddressNotificationManager());
            managers.Add(new OrderBookNotificationManager());

            managers.ForEach(_ => _.Send += Send);
        }
    }

    public class TransactionNotificationManager : INotificationManager
    {
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly Dictionary<ISession, SessionSubscriptor> subscriptors = new Dictionary<ISession, SessionSubscriptor>();

        public Action<ISession, NotificationMessage> Send { get; set; }

        public void ListenTo(ISession session, Topic topic)
        {
            var transactionFilter = (TransactionTopic)topic;
            if (transactionFilter == null)
                return;

            var hash = transactionFilter.Hash;

            using (locker.CreateLock())
            {
                subscriptors.GetOrCreate(session).Subscribe(hash);
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

        private List<JSON.API.Internals.Transaction> GetTransactions(SessionSubscriptor subscriptor, SignedLedger ledger)
        {
            return ledger.Ledger.Block.Transactions
                .Where(transaction => subscriptor.IsSubscribed(transaction, true))
                .Select(signed => TransactionConverter.GetTransaction(signed.Transaction)).ToList();
        }
    }

    internal class SessionSubscriptor
    {
        private readonly HashSet<TransactionHash> transactions = new HashSet<TransactionHash>();

        public void Subscribe(TransactionHash hash)
        {
            transactions.Add(hash);
        }

        public bool IsSubscribed(SignedTransaction transaction, bool unsubcribe = false)
        {
            var hash = transaction.Hash;
            if (unsubcribe)
                return transactions.Remove(hash);
            return transactions.Contains(hash);
        }
    }

    public class LedgerNotificationManager : INotificationManager
    {
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly HashSet<ISession> subscriptors = new HashSet<ISession>();

        public Action<ISession, NotificationMessage> Send { get; set; }

        public void ListenTo(ISession session, Topic topic)
        {
            var ledgerFilter = (LedgerTopic)topic;
            if (ledgerFilter == null)
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
                    Transactions = GetTransactions(ledger)
                };
                Send(subscriptor, new NotificationMessage(notification));
            }
        }

        private List<JSON.API.Internals.Transaction> GetTransactions(SignedLedger ledger)
        {
            return ledger.Ledger.Block.Transactions.Select(signed => TransactionConverter.GetTransaction(signed.Transaction)).ToList();
        }
    }

    public class AddressNotificationManager : INotificationManager
    {
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly Dictionary<ISession, AddressSubscriptor> subscriptors = new Dictionary<ISession, AddressSubscriptor>();

        public Action<ISession, NotificationMessage> Send { get; set; }

        public void ListenTo(ISession session, Topic topic)
        {
            var addressFilter = (AddressTopic) topic;
            if (addressFilter == null)
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

    public class OrderBookNotificationManager : INotificationManager
    {
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly Dictionary<ISession, HashSet<string>> subscriptors = new Dictionary<ISession, HashSet<string>>();

        public Action<ISession, NotificationMessage> Send { get; set; }

        public void ListenTo(ISession session, Topic topic)
        {
            var addressFilter = (OrderBookTopic)topic;
            if (addressFilter == null)
                return;

            var address = addressFilter.Symbol;

            using (locker.CreateLock())
            {
                subscriptors.GetOrCreate(session).Add(address);
            }

            throw new NotImplementedException();
        }

        public void Notify(SignedLedger ledger)
        {
            // TODO 
        }

    }

    public interface INotificationManager
    {
        void ListenTo(ISession session, Topic topic);
        void Notify(SignedLedger ledger);
        Action<ISession, NotificationMessage> Send { get; set; }
    }
}
