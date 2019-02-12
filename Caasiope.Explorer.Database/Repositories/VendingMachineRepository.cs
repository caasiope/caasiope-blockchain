using System.Collections.Generic;
using System.Data.Entity;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Explorer.Database.SQL.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Repositories
{
    public class VendingMachineRepository : Repository<VendingMachineEntity, vendingmachine, long>
    {
        private class VendingMachineIndex : Index
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

        private readonly VendingMachineIndex machines = new VendingMachineIndex();

        public VendingMachineRepository()
        {
            RegisterIndex(machines);
        }

        protected override vendingmachine ToEntity(VendingMachineEntity item)
        {
            var account = item.Account.Address.ToRawBytes();

            return new vendingmachine
            {
                declaration_id = item.DeclarationId,
                account = account,
                owner = item.Account.Owner.ToRawBytes(),
                currency_in = item.Account.CurrencyIn,
                currency_out = item.Account.CurrencyOut,
                rate = item.Account.Rate,
            };
        }

        protected override VendingMachineEntity ToItem(vendingmachine entity)
        {
            var address = Address.FromRawBytes(entity.account);
            var owner = Address.FromRawBytes(entity.owner);
            var input = (Currency)entity.currency_in;
            var output = (Currency)entity.currency_out;
            var rate = (Amount)entity.rate;
            return new VendingMachineEntity(entity.declaration_id, new VendingMachineAccount(address, owner, input, output, rate));
        }

        protected override DbSet<vendingmachine> GetDbSet(ExplorerEntities entities)
        {
            return entities.vendingmachines;
        }
		
        protected override long GetKey(VendingMachineEntity item)
        {
            return item.DeclarationId;
        }

        public VendingMachineEntity GetByAddress(Address address)
        {
            return machines.GetByAddress(address)?.Item;
        }
    }
}