using System.Collections.Generic;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Notifications
{
    public class FundsNotification : Notification
    {
        public long Height;
        public Dictionary<string, decimal> Funds;
    }
}