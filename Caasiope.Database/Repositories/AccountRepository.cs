using System.Data.Entity;
using System.Linq;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.Types;
using AccountEntity = Caasiope.Database.Repositories.Entities.AccountEntity;

namespace Caasiope.Database.Repositories
{
    public class AccountRepository : Repository<AccountEntity, account>
    {
        protected override bool CheckIsNew(BlockchainEntities entities, account item)
        {
            // TODO this is a workaround!
            return entities.accounts.AsNoTracking().SingleOrDefault(_ => _.address == item.address) == null;
        }

        protected override DbSet<account> GetDbSet(BlockchainEntities entities)
        {
            return entities.accounts;
        }

        protected override account ToEntity(AccountEntity item)
        {
            return new account()
            {
                address = item.Address.ToRawBytes(),
                raw = item.Raw
            };
        }

        protected override AccountEntity ToItem(account entity)
        {
            return new AccountEntity(Address.FromRawBytes(entity.address), entity.raw);
        }
    }
}