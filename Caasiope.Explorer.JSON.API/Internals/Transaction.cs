using System.Collections.Generic;
using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class Transaction
    {
        public string Hash;
        public long Expire;
        public List<TxDeclaration> Declarations;
        public List<TxInput> Inputs;
        public List<TxOutput> Outputs;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Message;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TxInput Fees;
    }

    public class HistoricalTransaction
    {
        public long Height;
        public Transaction Transaction;
    }
}
