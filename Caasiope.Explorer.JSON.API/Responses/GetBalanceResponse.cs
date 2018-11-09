using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Requests;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Responses
{
    public class GetBalanceResponse : Response<GetBalanceRequest>
    {
        public string Address;
        public Dictionary<string, decimal> Balance;
    }
}