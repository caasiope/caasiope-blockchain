using System.Collections.Generic;
using System.Linq;

namespace Caasiope.Protocol.Types
{
    public class SignedTransaction : Signed<Transaction, TransactionHash>
    {
        public Transaction Transaction => Data;

        // TODO in some cases signatures could be not sorted
        public SignedTransaction(Transaction transaction, List<Signature> signatures) : base(transaction, transaction.GetHash(), SortSignatures(signatures)) { }

        private static List<Signature> SortSignatures(List<Signature> signatures)
        {
            if (signatures.Count == 1)
                return signatures;

            var list = new SortedList<PublicKey, Signature>();
            foreach (var signature in signatures)
            {
                list.Add(signature.PublicKey, signature);
            }

            return list.Values.ToList();
        }

        public SignedTransaction(Transaction transaction) : base(transaction, transaction.GetHash(), new List<Signature>()) { }

        public override bool Equals(object obj)
        {
            return obj is SignedTransaction t2 && Hash.Equals(t2.Hash);
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }
    }
}