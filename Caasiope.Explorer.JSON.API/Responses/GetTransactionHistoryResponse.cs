using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Requests;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Responses
{
    public class GetTransactionHistoryResponse : Response<GetTransactionHistoryRequest>
    {
        public List<Internals.HistoricalTransaction> Transactions;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Total;
    }
}