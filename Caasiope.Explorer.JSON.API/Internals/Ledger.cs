using System.Collections.Generic;
using Newtonsoft.Json;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class Ledger
    {
        public long Height;
        public string Hash;
        public long Timestamp;
        public string Lastledger;
        public byte Version;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public short? FeeTransactionIndex;
        public List<TransactionHeader> Transactions;

        public Ledger(long height, string hash, long timestamp, string lastledger, byte version, short? feeTransactionIndex, List<TransactionHeader> transactions)
        {
            Height = height;
            Hash = hash;
            Timestamp = timestamp;
            Lastledger = lastledger;
            Version = version;
            FeeTransactionIndex = feeTransactionIndex;
            Transactions = transactions;
        }
    }
}