using System.Data.Entity;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Explorer.Database.SQL.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Repositories
{
    public class SecretRevelationRepository : Repository<SecretRevelationEntity, secretrevelation, long>
    {
        protected override secretrevelation ToEntity(SecretRevelationEntity item)
        {
            return new secretrevelation
            {
                declaration_id = item.DeclarationId,
                secret = item.SecretRevelation.Secret.Bytes
            };
        }

        protected override SecretRevelationEntity ToItem(secretrevelation entity)
        {
            return new SecretRevelationEntity(entity.declaration_id, new SecretRevelation(new Secret(entity.secret)));
        }

        protected override DbSet<secretrevelation> GetDbSet(ExplorerEntities entities)
        {
            return entities.secretrevelations;
        }
		
        protected override long GetKey(SecretRevelationEntity item)
        {
            return item.DeclarationId;
        }
    }
}
