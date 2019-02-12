using System.Collections.Generic;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Notifications
{
    public class AddressNotification : Notification
    {
        public string Address;
        public Dictionary<string, decimal> Balance;
        public long Height;
        public List<string> Transactions;
    }
}