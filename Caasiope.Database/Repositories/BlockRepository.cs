using System.Data.Entity;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;

namespace Caasiope.Database.Repositories
{
    public class BlockRepository : Repository<BlockEntity, block, long>
    {
        protected override long GetKey(BlockEntity item)
        {
            return item.LedgerHeight;
        }

        protected override block ToEntity(BlockEntity item)
        {
            return new block
            {
                ledger_height = item.LedgerHeight,
                hash = item.Hash,
                fee_transaction_index = item.FeeTransactionIndex,
            };
        }

        protected override BlockEntity ToItem(block entity)
        {
            return new BlockEntity(entity.ledger_height, entity.hash, entity.fee_transaction_index);
        }


        protected override DbSet<block> GetDbSet(BlockchainEntities entities)
        {
            return entities.blocks;
        }
    }
}