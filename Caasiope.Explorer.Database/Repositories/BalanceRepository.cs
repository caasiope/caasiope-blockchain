using System.Data.Entity;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Explorer.Database.SQL.Entities;
using Caasiope.Protocol.Types;
using AccountBalanceFull = Caasiope.Explorer.Database.Repositories.Entities.AccountBalanceFull;

namespace Caasiope.Explorer.Database.Repositories
{
    public class BalanceRepository : Repository<Entities.AccountBalanceFull, balance, string>
    {
        protected override string GetKey(Entities.AccountBalanceFull item)
        {
            return item.Account.Encoded + Currency.ToSymbol(item.AccountBalance.Currency);
        }

        protected override balance ToEntity(Entities.AccountBalanceFull item)
        {
            return new balance
            {
                account = item.Account.ToRawBytes(),
                amount = item.AccountBalance.Amount,
                currency = item.AccountBalance.Currency,
            };
        }

        protected override Entities.AccountBalanceFull ToItem(balance entity)
        {
            var address = Address.FromRawBytes(entity.account);
            var amount = (Amount)entity.amount;
            var currency = (Currency)entity.currency;
            return new Entities.AccountBalanceFull(address, new AccountBalance(currency, amount));
        }

        protected override DbSet<balance> GetDbSet(BlockchainEntities entities)
        {
            return entities.balances;
        }
    }
}
