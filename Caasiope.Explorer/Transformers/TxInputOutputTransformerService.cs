using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;

namespace Caasiope.Explorer.Transformers
{
    internal class TxInputOutputTransformerService : DataTransformerService<TxInputOutputFull, TransactionInputOutputRepository>
    {
        protected override IEnumerable<TxInputOutputFull> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;
            var list = new List<TxInputOutputFull>();
            var transactions = signedLedgerState.Ledger.Ledger.Block.Transactions;
            foreach (var transaction in transactions)
            {
                var hash = transaction.Hash;

                if (transaction.Transaction.Fees != null)
                    list.Add(new TxInputOutputFull(transaction.Transaction.Fees, hash, 0));

                byte index = 1;
                foreach (var input in transaction.Transaction.Inputs)
                {
                    list.Add(new TxInputOutputFull(input, hash, index++));
                }

                foreach (var input in transaction.Transaction.Outputs)
                {
                    list.Add(new TxInputOutputFull(input, hash, index++));
                }
            }

            return list;
        }
    }
}