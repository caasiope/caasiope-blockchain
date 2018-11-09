using Caasiope.JSON.API.Requests;
using Helios.JSON;

namespace Caasiope.JSON
{
	public class BlockchainApi : JsonApi
	{
		public BlockchainApi() : base(typeof(GetSignedLedgerRequest).Assembly)
		{
		}
	}
}
