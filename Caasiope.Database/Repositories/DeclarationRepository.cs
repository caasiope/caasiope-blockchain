using System.Data.Entity;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories
{
    public class DeclarationRepository : Repository<DeclarationEntity, declaration, long>
    {
        protected override declaration ToEntity(DeclarationEntity item)
        {
            return new declaration
            {
                declaration_id = item.DeclarationId,
                type = (byte)item.DeclarationType,
            };
        }

        protected override DeclarationEntity ToItem(declaration entity)
        {
            return new DeclarationEntity(entity.declaration_id, (DeclarationType)entity.type);
        }

        protected override DbSet<declaration> GetDbSet(BlockchainEntities entities)
        {
            return entities.declarations;
        }

        protected override long GetKey(DeclarationEntity item)
        {
            return item.DeclarationId;
        }
    }
}