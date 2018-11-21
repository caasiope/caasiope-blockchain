using System.Data.Entity;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Explorer.Database.SQL.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Repositories
{
    public class TransactionMessageRepository : Repository<TransactionMessageEntity, transactionmessage, TransactionHash>
    {
        protected override DbSet<transactionmessage> GetDbSet(BlockchainEntities entities)
        {
            return entities.transactionmessages;
        }

        protected override transactionmessage ToEntity(TransactionMessageEntity item)
        {
            return new transactionmessage() {message = item.Message.GetBytes(), transaction_hash = item.Hash.Bytes};
        }

        protected override TransactionMessageEntity ToItem(transactionmessage entity)
        {
            return new TransactionMessageEntity(new TransactionHash(entity.transaction_hash), new TransactionMessage(entity.message));
        }

        protected override TransactionHash GetKey(TransactionMessageEntity item)
        {
            return item.Hash;
        }
    }
}
