using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Notifications
{
    public class LedgerNotification : Notification
    {
        public long Height;
        public long Timestamp;
        public string Hash;
        public int Transactions;
    }
}