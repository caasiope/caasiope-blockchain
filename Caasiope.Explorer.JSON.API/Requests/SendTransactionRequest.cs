using System.Collections.Generic;
using Caasiope.Explorer.JSON.API.Internals;
using Caasiope.Explorer.JSON.API.Responses;
using Helios.JSON;

namespace Caasiope.Explorer.JSON.API.Requests
{
    public class SendTransactionRequest : Request<SendTransactionResponse>
    {
        public Transaction Transaction;
        public List<Signature> Signatures;
    }
}