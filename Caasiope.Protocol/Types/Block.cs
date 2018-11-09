using System.Collections.Generic;
using System.Linq;
using Caasiope.NBitcoin;

namespace Caasiope.Protocol.Types
{
    public class Block
    {
        private readonly MerkleNode<TransactionHash> merkleRoot;

        public readonly IEnumerable<SignedTransaction> Transactions;
        public readonly long LedgerHeight;
        public readonly BlockHash Hash;
        /// TODO It must be ushort 
        public readonly short? FeeTransactionIndex;
        // public readonly SignedTransaction FeeTransaction;

        public static Block CreateBlock(long ledgerHeight, List<SignedTransaction> transactions, short? index)
        {
            return new Block(ledgerHeight, SortTransactions(transactions), index);
        }

        public static Block CreateBlock(long ledgerHeight, List<SignedTransaction> transactions, SignedTransaction feeTransaction = null)
        {
            var sorted = SortTransactions(transactions);
            if (feeTransaction != null)
            {
                sorted.Add(feeTransaction.Hash, feeTransaction);
            }
            return new Block(ledgerHeight, sorted, GetFeeTransactionIndex(sorted, feeTransaction));
        }

        private static SortedList<TransactionHash, SignedTransaction> SortTransactions(IEnumerable<SignedTransaction> transactions)
        {
            var list = new SortedList<TransactionHash,SignedTransaction>();
            foreach (var transaction in transactions)
            {
                list.Add(transaction.Hash, transaction);
            }
            return list;
        }

        // transactions need to be already sorted
        private Block(long ledgerHeight, SortedList<TransactionHash,SignedTransaction> transactions, short? index)
        {
            LedgerHeight = ledgerHeight;
            FeeTransactionIndex = index;
            Transactions = transactions.Values.ToList();

            // TODO fee transaction index?
            // TODO LedgerHeight?
            merkleRoot = MerkleNode<TransactionHash>.GetRoot(Transactions.Select(t => t.Hash));
            Hash = new BlockHash(merkleRoot.Hash.Bytes);
        }

        private static short? GetFeeTransactionIndex(SortedList<TransactionHash, SignedTransaction> transactions, SignedTransaction fees)
        {
            if (fees == null)
                return null;
            return (short) transactions.IndexOfKey(fees.Hash);
        }
    }

    public class BlockHash : Hash256 { public BlockHash(byte[] bytes) : base(bytes) { } }
}