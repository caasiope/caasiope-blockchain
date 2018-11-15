using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Transformers
{
    internal class AccountTransformerService : DataTransformerService<Database.Repositories.Entities.AccountEntity, AccountRepository>
    {
        protected override IEnumerable<Database.Repositories.Entities.AccountEntity> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;
            var list = new List<Database.Repositories.Entities.AccountEntity>();
            var accounts = signedLedgerState.State.Accounts;
            foreach (var account in accounts)
            {
                list.Add(new Database.Repositories.Entities.AccountEntity(account.Address, account.CurrentLedgerHeight, account.IsDeclared));
            }
            return list;
        }
    }
}