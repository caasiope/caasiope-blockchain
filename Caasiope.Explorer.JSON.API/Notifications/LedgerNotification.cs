using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Internals;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Notifications
{
    public class LedgerNotification : Notification
    {
        public long Height;
        public long Timestamp;
        public string Hash;
        public Dictionary<string, decimal> Funds;
        public List<Transaction> Transactions;
    }
}