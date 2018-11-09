using System.Collections.Generic;
using System.Linq;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;

namespace Caasiope.Node.Transformers
{
    internal class DeclarationTransformerService : DataTransformerService<DeclarationEntity, DeclarationRepository>
    {
        protected override IEnumerable<DeclarationEntity> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;

            var list = new List<DeclarationEntity>();
            var transactions = signedLedgerState.Ledger.Ledger.Block.Transactions;
            var declarations = context.GetDeclarations();
            var index = 0;
            foreach (var transaction in transactions)
            {
                foreach (var declaration in transaction.Transaction.Declarations)
                {
                    list.Add(new DeclarationEntity(GetDeclarationId(declarations.Declarations, index), declaration.Type));
                    index++;
                }
            }
            return list.Distinct();
        }


        private long GetDeclarationId(List<TransactionDeclarationEntity> declarations, int index)
        {
            return declarations[index].DeclarationId;
        }
    }
}