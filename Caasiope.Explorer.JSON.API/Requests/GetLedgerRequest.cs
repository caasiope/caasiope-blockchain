using Caasiope.Explorer.JSON.API.Responses;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Requests
{
    public class GetLedgerRequest : Request<GetLedgerResponse>
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? Height;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Hash;
    }
}