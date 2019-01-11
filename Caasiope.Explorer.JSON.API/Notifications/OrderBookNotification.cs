using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Internals;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Notifications
{
    public class OrderBookNotification : Notification
    {
        public string Symbol;
        public List<Order> Orders;
    }
}