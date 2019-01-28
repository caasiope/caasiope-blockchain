using Caasiope.Explorer.JSON.API.Converters;
using Caasiope.Explorer.JSON.API.Requests;
using Helios.JSON;
using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API
{
	public class BlockchainExplorerApi : JsonApi
	{
		public BlockchainExplorerApi() : base(typeof(GetBalanceRequest).Assembly, new JsonConverter[] { new TopicConverter(), new DeclarationConverter() })
		{
		}
	}
}
