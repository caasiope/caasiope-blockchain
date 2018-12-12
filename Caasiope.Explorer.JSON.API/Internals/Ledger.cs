using System.Collections.Generic;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class Ledger
    {
        public long Height;
        public string Hash;
        public long Timestamp;
        public string Lastledger;
        public byte Version;
        public List<string> Transactions;

        public Ledger(long height, string hash, long timestamp, string lastledger, byte version, List<string> transactions)
        {
            Height = height;
            Hash = hash;
            Timestamp = timestamp;
            Lastledger = lastledger;
            Version = version;
            Transactions = transactions;
        }
    }
}