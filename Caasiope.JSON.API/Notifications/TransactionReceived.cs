using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.JSON.API.Notifications
{
    [JsonObject]
    public class TransactionReceived : Notification
    {
        [JsonProperty]
        public string Transaction;
    }
}