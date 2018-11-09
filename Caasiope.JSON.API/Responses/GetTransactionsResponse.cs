using System.Collections.Generic;
using Caasiope.JSON.API.Requests;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.JSON.API.Responses
{
    [JsonObject]
    public class GetTransactionsResponse : Response<GetAccountRequest>
    {
        [JsonProperty]
        public List<string> Transactions;
    }
}
