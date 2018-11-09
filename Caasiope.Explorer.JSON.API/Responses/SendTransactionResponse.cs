using Caasiope.Explorer.JSON.API.Requests;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Responses
{
    public class SendTransactionResponse : Response<SendTransactionRequest>
    {
        public string Hash;
    }
}