using Caasiope.Explorer.JSON.API.Responses;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Requests
{
	public class GetTransactionHistoryRequest : Request<GetTransactionHistoryResponse>
	{
		public string Address;
	    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? Height;
		public int Count;
		public int Page;
	}
}