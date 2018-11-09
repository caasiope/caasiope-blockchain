using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Transformers
{
    internal class TransactionTransformerService : DataTransformerService<SignedTransactionSimple, TransactionRepository>
    {
        protected override IEnumerable<SignedTransactionSimple> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;
            var list = new List<SignedTransactionSimple>();
            var transactions = signedLedgerState.Ledger.Ledger.Block.Transactions;
            foreach (var transaction in transactions)
            {
                list.Add(new SignedTransactionSimple(transaction.Hash, signedLedgerState.Ledger.Ledger.LedgerLight.Height, transaction.Transaction.Expire));
            }
            return list;
        }
    }
}