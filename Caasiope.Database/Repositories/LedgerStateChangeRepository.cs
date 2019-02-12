using System.Data.Entity;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;

namespace Caasiope.Database.Repositories
{
    public class LedgerStateChangeRepository : Repository<LedgerStateChangeSimple, ledgerstatechange>
    {
        protected override ledgerstatechange ToEntity(LedgerStateChangeSimple item)
        {
            return new ledgerstatechange
            {
                ledger_height = item.LedgerHeight,
                raw = item.Raw
            };
        }

        protected override LedgerStateChangeSimple ToItem(ledgerstatechange entity)
        {
            return new LedgerStateChangeSimple(entity.ledger_height, entity.raw);
        }

        protected override bool CheckIsNew(BlockchainEntities entities, LedgerStateChangeSimple item)
        {
            return true;
        }

        protected override DbSet<ledgerstatechange> GetDbSet(BlockchainEntities entities)
        {
            return entities.ledgerstatechanges;
        }
    }
}