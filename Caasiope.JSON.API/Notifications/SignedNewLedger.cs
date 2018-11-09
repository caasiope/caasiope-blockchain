using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.JSON.API.Notifications
{
    [JsonObject]
    public class SignedNewLedger : Notification
    {
        [JsonProperty]
        public string Ledger;
    }
}