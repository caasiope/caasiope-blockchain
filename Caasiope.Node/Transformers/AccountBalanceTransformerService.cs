using System.Collections.Generic;
using Caasiope.Database.Repositories;

namespace Caasiope.Node.Transformers
{
    internal class AccountBalanceTransformerService : DataTransformerService<Database.Repositories.Entities.AccountBalanceFull, BalanceRepository>
    {
        protected override IEnumerable<Database.Repositories.Entities.AccountBalanceFull> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;
            var list = new List<Database.Repositories.Entities.AccountBalanceFull>();
            var balances = signedLedgerState.State.Balances;
            foreach (var balance in balances)
            {
                list.Add(new Database.Repositories.Entities.AccountBalanceFull(balance.Account, balance.AccountBalance));
            }
            return list;
        }
    }
}