using Newtonsoft.Json;
using System.Collections.Generic;
using Caasiope.JSON.API.Responses;
using Helios.JSON;

namespace Caasiope.JSON.API.Requests
{
    [JsonObject]
    public class GetTransactionsRequest : Request<GetTransactionsResponse>
    {
        [JsonProperty]
        public List<string> Hashes;
    }
}
