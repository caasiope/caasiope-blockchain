using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Transformers
{
    internal class TransactionSignatureTransformerService : DataTransformerService<TransactionSignature, TransactionSignatureRepository>
    {
        protected override IEnumerable<TransactionSignature> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;
            var list = new List<TransactionSignature>();
            var transactions = signedLedgerState.Ledger.Ledger.Block.Transactions;
            foreach (var transaction in transactions)
            {
                var hash = transaction.Hash;
                foreach (var signature in transaction.Signatures)
                {
                    list.Add(new TransactionSignature(signature, hash));
                }
            }
            return list;
        }
    }
}