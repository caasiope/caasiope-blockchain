using Caasiope.Explorer.JSON.API.Responses;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Requests
{
	public class GetTransactionRequest : Request<GetTransactionResponse>
	{
		public string Hash;
	}
}