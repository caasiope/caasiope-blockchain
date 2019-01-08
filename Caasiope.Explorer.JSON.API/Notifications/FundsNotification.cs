using System.Collections.Generic;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Notifications
{
    public class FundsNotification : Notification
    {
        public Dictionary<string, decimal> Funds;
        public long Height;
    }
}