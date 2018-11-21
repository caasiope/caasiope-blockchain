using System.Data.Entity;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.Types;
using AccountEntity = Caasiope.Database.Repositories.Entities.AccountEntity;

namespace Caasiope.Database.Repositories
{
    public class AccountRepository : Repository<AccountEntity, account, Address>
    {
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

        protected override Address GetKey(AccountEntity item)
        {
            return item.Address;
        }
    }
}