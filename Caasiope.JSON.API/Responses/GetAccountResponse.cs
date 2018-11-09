using Caasiope.JSON.API.Requests;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.JSON.API.Responses
{
    [JsonObject]
    public class GetAccountResponse : Response<GetAccountRequest>
    {
        [JsonProperty]
        public string Account;
    }
}
