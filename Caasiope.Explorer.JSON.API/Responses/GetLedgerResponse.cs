using Caasiope.Explorer.JSON.API.Internals;
using Caasiope.Explorer.JSON.API.Requests;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Responses
{
    public class GetLedgerResponse : Response<GetLedgerRequest>
    {
        public Ledger Ledger;
    }
}