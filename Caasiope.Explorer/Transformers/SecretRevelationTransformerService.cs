using System.Collections.Generic;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Transformers
{
    internal class SecretRevelationTransformerService : ExplorerDataTransformerService<SecretRevelationEntity, SecretRevelationRepository>
    {
        protected override IEnumerable<SecretRevelationEntity> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;
            var list = new List<SecretRevelationEntity>();
            var transactions = signedLedgerState.Ledger.Ledger.Block.Transactions;
            var declarations = context.GetDeclarations();
            var index = 0;
            foreach (var transaction in transactions)
            {
                foreach (var declaration in transaction.Transaction.Declarations)
                {
                    if (declaration.Type == DeclarationType.Secret)
                    {
                        var secret = (SecretRevelation)declaration;
                        list.Add(new SecretRevelationEntity(GetDeclarationId(declarations.Declarations, index), new SecretRevelation(secret.Secret)));
                    }
                    index++;
                }
            }
            return list;
        }

        private long GetDeclarationId(List<TransactionDeclarationEntity> declarations, int index)
        {
            return declarations[index].DeclarationId;
        }
    }
}