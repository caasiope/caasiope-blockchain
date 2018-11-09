using Caasiope.JSON.API.Responses;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.JSON.API.Requests
{
    [JsonObject]
    public class GetSignedLedgerRequest : Request<GetSignedLedgerResponse>
    {
        [JsonProperty]
        public long Height;
    }
}