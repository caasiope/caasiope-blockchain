using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Internals;
using Caasiope.Explorer.JSON.API.Requests;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Responses
{
    public class GetAccountResponse : Response<GetAccountRequest>
    {
        public string Address;
        public Dictionary<string, decimal> Balance;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TxDeclaration Declaration;
    }
}