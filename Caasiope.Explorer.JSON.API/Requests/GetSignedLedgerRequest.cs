using Caasiope.Explorer.JSON.API.Responses;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Requests
{
    public class GetSignedLedgerRequest : Request<GetSignedLedgerResponse>
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? Height;
    }
}