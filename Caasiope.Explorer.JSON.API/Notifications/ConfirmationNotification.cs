using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Internals;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Notifications
{
    public class ConfirmationNotification : Notification
    {
        public long Height;
        public long Timestamp;
        public string Hash;
        public List<Transaction> Transactions;
    }
}