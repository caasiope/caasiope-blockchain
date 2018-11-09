using System.Collections.Generic;
using System.Data.Entity;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories
{
    public class TimeLockRepository : Repository<TimeLockEntity, timelock, long>
    {
        private class TimeLockIndex : Index
        {
            readonly Dictionary<Address, Wrapper> cache = new Dictionary<Address, Wrapper>();

            protected internal override void Add(Wrapper item)
            {
                cache.Add(item.Item.Account.Address, item);
            }

            public Wrapper GetByAddress(Address address)
            {
                Wrapper wrapper;
                if (!cache.TryGetValue(address, out wrapper))
                    return null;
                return wrapper;
            }
        }

        private readonly TimeLockIndex timelocks = new TimeLockIndex();

        public TimeLockRepository()
        {
            RegisterIndex(timelocks);
        }

        protected override timelock ToEntity(TimeLockEntity item)
        {
            var account = item.Account.Address.ToRawBytes();

            return new timelock
            {
                declaration_id = item.DeclarationId,
                account = account,
                timestamp = item.Account.Timestamp
            };
        }

        protected override TimeLockEntity ToItem(timelock entity)
        {
            var address = Address.FromRawBytes(entity.account);
            return new TimeLockEntity(entity.declaration_id, new TimeLockAccount(address, entity.timestamp));
        }

        protected override DbSet<timelock> GetDbSet(BlockchainEntities entities)
        {
            return entities.timelocks;
        }
		
        protected override long GetKey(TimeLockEntity item)
        {
            return item.DeclarationId;
        }

        public TimeLockEntity GetByAddress(Address address)
        {
            return timelocks.GetByAddress(address)?.Item;
        }
    }
}