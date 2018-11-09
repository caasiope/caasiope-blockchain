using Caasiope.JSON.API.Requests;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.JSON.API.Responses
{
	[JsonObject]
	public class GetCurrentLedgerHeightResponse : Response<GetCurrentLedgerHeightRequest>
	{
		[JsonProperty]
		public long Height;
	}
}