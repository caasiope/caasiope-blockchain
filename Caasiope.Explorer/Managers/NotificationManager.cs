using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.JSON.API;
using Caasiope.Explorer.JSON.API.Notifications;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;
using Helios.JSON;

namespace Caasiope.Explorer.Managers
{
    // subscription manager ?
    public class NotificationManager
    {
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly Dictionary<ISession, SessionSubscriptor> subscriptors = new Dictionary<ISession, SessionSubscriptor>();

        public Action<ISession, NotificationMessage> Send;

        public void ListenTo(ISession session, TransactionHash hash)
        {
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
                    Hash = ledger.Hash.ToString(),
                    Height = ledger.GetHeight(),
                    Timestamp = ledger.GetTimestamp(),
                    Transactions = GetTransactions(subscriptor.Value, ledger)
                };
                Send(subscriptor.Key, new NotificationMessage(notification));
            }
        }

        private List<Explorer.JSON.API.Internals.Transaction> GetTransactions(SessionSubscriptor subscriptor, SignedLedger ledger)
        {
            return ledger.Ledger.Block.Transactions
                .Where(transaction => subscriptor.IsSubscribed(transaction,true))
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
}
