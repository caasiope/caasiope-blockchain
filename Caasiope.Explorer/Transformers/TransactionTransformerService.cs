using System.Collections.Generic;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Explorer.Database.Repositories.Entities;

namespace Caasiope.Explorer.Transformers
{
    internal class TransactionTransformerService : ExplorerDataTransformerService<SignedTransactionSimple, TransactionRepository>
    {
        protected override IEnumerable<SignedTransactionSimple> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;
            var list = new List<SignedTransactionSimple>();
            var transactions = signedLedgerState.Ledger.Ledger.Block.Transactions;
            foreach (var transaction in transactions)
            {
                list.Add(new SignedTransactionSimple(transaction.Hash, signedLedgerState.Ledger.Ledger.LedgerLight.Height, transaction.Transaction.Expire, signedLedgerState.Ledger.Ledger.LedgerLight.Timestamp));
            }
            return list;
        }
    }
}