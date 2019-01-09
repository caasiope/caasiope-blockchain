using Caasiope.Explorer.JSON.API.Responses;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Requests
{
    public class GetAccountRequest : Request<GetAccountResponse>
    {
        public string Address;
    }
}