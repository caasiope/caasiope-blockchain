using System.Collections.Generic;
using System.Linq;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Trackers
{
    public class TransactionTracker
    {
        private readonly Dictionary<TransactionHash, SignedTransaction> transactions = new Dictionary<TransactionHash, SignedTransaction>();

        public bool TryAdd(SignedTransaction transaction)
        {
            var hash = transaction.Hash;
            if (transactions.ContainsKey(hash))
                return false;
            transactions.Add(hash, transaction);
            return true;
        }

        public void Add(SignedTransaction transaction)
        {
            var hash = transaction.Hash;
            transactions.Add(hash, transaction);
        }

        public bool TryGetTransaction(TransactionHash hash, out SignedTransaction transaction)
        {
            return transactions.TryGetValue(hash, out transaction);
        }

        public bool IsExist(TransactionHash hash)
        {
            return transactions.ContainsKey(hash);
        }

        public List<TransactionHash> GetAllHashes()
        {
            return transactions.Keys.ToList();
        }
    }
}
