using System.Data.Entity;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.MerkleTrees;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories
{
    internal class LedgerRepository : Repository<LedgerEntity, ledger, long>
    {
        protected override long GetKey(LedgerEntity item)
        {
            return item.Ledger.Height;
        }

        protected override ledger ToEntity(LedgerEntity item)
        {
            return new ledger
            {
                height = item.Ledger.Height,
                timestamp = item.Ledger.Timestamp,
                hash = item.Hash.Bytes,
                merkle_root_hash = item.MerkleRootHash.Bytes,
                previous_hash = item.Ledger.Lastledger.Bytes,version = item.Ledger.Version.VersionNumber,
                raw = item.Raw,
            };
        }

        protected override LedgerEntity ToItem(ledger entity)
        {
            var ledger = new LedgerLight(entity.height, entity.timestamp, new LedgerHash(entity.previous_hash), new ProtocolVersion(entity.version));
            return new LedgerEntity(new LedgerHash(entity.hash), ledger, new LedgerMerkleRootHash(entity.merkle_root_hash), entity.raw);
        }


        protected override DbSet<ledger> GetDbSet(BlockchainEntities entities)
        {
            return entities.ledgers;
        }
    }
}