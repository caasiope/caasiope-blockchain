using Caasiope.JSON.API.Requests;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.JSON.API.Responses
{
    [JsonObject]
    public class GetSignedLedgerResponse : Response<GetSignedLedgerRequest>
    {
        [JsonProperty]
        public string Ledger;
    }
}