using System.Collections.Generic;
using System.Data.Entity;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories
{
    public class TransactionDeclarationRepository : Repository<TxDeclarationEntity, transactiondeclaration, Address>
    {
        protected override transactiondeclaration ToEntity(TxDeclarationEntity item)
        {
            var address = item.Address.ToRawBytes();
            return new transactiondeclaration
            {
                address = address,
                raw = item.Raw,
            };
        }

        protected override TxDeclarationEntity ToItem(transactiondeclaration entity)
        {
            var address = Address.FromRawBytes(entity.address);
            return new TxDeclarationEntity(address, entity.raw);
        }

        protected override DbSet<transactiondeclaration> GetDbSet(BlockchainEntities entities)
        {
            return entities.transactiondeclarations;
        }

        protected override Address GetKey(TxDeclarationEntity item)
        {
            return item.Address;
        }
    }
}
