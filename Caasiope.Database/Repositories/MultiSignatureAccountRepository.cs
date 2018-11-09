using System.Collections.Generic;
using System.Data.Entity;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories
{
    public class MultiSignatureAccountRepository : Repository<MultiSignatureEntity, multisignatureaccount, long>
    {
        private class MultiSignatureIndex : Index
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

        private readonly MultiSignatureIndex multisigs = new MultiSignatureIndex();

        public MultiSignatureAccountRepository()
        {
            RegisterIndex(multisigs);
        }

        protected override multisignatureaccount ToEntity(MultiSignatureEntity item)
        {
            var account = item.Account.Address.ToRawBytes();

            return new multisignatureaccount
            {
                declaration_id = item.DeclarationId,
                hash = item.Hash.Bytes,
                account = account,
                required = item.Account.Required
            };
        }

        protected override MultiSignatureEntity ToItem(multisignatureaccount entity)
        {
            var address = Address.FromRawBytes(entity.account);
            return new MultiSignatureEntity(entity.declaration_id, new MultiSignatureHash(entity.hash), new MultiSignatureAccount(address, entity.required));
        }

        protected override DbSet<multisignatureaccount> GetDbSet(BlockchainEntities entities)
        {
            return entities.multisignatureaccounts;
        }

        protected override long GetKey(MultiSignatureEntity item)
        {
            return item.DeclarationId;
        }

        public MultiSignatureEntity GetByAddress(Address address)
        {
            return multisigs.GetByAddress(address)?.Item;
        }
    }
}
