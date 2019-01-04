using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Internals;
using Caasiope.Explorer.JSON.API.Requests;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Responses
{
    public class GetLatestLedgersResponse : Response<GetLatestLedgersRequest>
    {
        public List<Ledger> Ledgers;
    }
}