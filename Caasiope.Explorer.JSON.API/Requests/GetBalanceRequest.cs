using Caasiope.Explorer.JSON.API.Responses;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Requests
{
    public class GetBalanceRequest : Request<GetBalanceResponse>
    {
        public string Address;
    }
}
