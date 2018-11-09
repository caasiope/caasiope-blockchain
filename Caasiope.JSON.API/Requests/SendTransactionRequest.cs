using Caasiope.JSON.API.Responses;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.JSON.API.Requests
{
    [JsonObject]
    public class SendTransactionRequest : Request<SendTransactionResponse>
    {
        [JsonProperty]
        public string Transaction;
    }
}
