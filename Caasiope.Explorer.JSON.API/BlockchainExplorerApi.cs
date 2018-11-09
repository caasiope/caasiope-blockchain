using Caasiope.Explorer.JSON.API.Requests;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API
{
	public class BlockchainExplorerApi : JsonApi
	{
		public BlockchainExplorerApi() : base(typeof(GetBalanceRequest).Assembly)
		{
		}
	}
}
