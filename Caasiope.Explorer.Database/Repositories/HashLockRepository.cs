using System.Collections.Generic;
using System.Data.Entity;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Explorer.Database.SQL.Entities;
using Caasiope.Protocol.Types;
using Caasiope.NBitcoin;

namespace Caasiope.Explorer.Database.Repositories
{
    public class HashLockRepository : Repository<HashLockEntity, hashlock, long>
    {
        private class HashLockIndex : Index
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

        private readonly HashLockIndex hashlocks = new HashLockIndex();

        public HashLockRepository()
        {
            RegisterIndex(hashlocks);
        }

        protected override hashlock ToEntity(HashLockEntity item)
        {
            var account = item.Account.Address.ToRawBytes();

            return new hashlock
            {
                declaration_id = item.DeclarationId,
                account = account,
                secret_type = (byte) item.Account.SecretHash.Type,
                secret_hash = item.Account.SecretHash.Hash.Bytes,
            };
        }

        protected override HashLockEntity ToItem(hashlock entity)
        {
            var address = Address.FromRawBytes(entity.account);
            return new HashLockEntity(entity.declaration_id, new HashLockAccount(address, new SecretHash((SecretHashType)entity.secret_type, new Hash256(entity.secret_hash))));
        }

        protected override DbSet<hashlock> GetDbSet(ExplorerEntities entities)
        {
            return entities.hashlocks;
        }

        protected override long GetKey(HashLockEntity item)
        {
            return item.DeclarationId;
        }

        public HashLockEntity GetByAddress(Address address)
        {
            return hashlocks.GetByAddress(address)?.Item;
        }
    }
}
