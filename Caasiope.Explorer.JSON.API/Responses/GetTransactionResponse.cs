using Caasiope.Explorer.JSON.API.Requests;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Responses
{
	public class GetTransactionResponse : Response<GetTransactionRequest>
	{
		public Internals.Transaction Transaction;
	}
}