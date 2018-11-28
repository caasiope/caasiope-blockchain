//using System.Data.Entity;
//using Caasiope.Explorer.Database.Repositories.Entities;
//using Caasiope.Explorer.Database.SQL;
//using Caasiope.Explorer.Database.SQL.Entities;

//namespace Caasiope.Explorer.Database.Repositories
//{
//    public class LedgerStateChangeRepository : Repository<LedgerStateChangeSimple, ledgerstatechange, long>
//    {
//        protected override long GetKey(LedgerStateChangeSimple item)
//        {
//            return item.LedgerHeight;
//        }

//        protected override ledgerstatechange ToEntity(LedgerStateChangeSimple item)
//        {
//            return new ledgerstatechange
//            {
//                ledger_height = item.LedgerHeight,
//                raw = item.Raw
//            };
//        }

//        protected override LedgerStateChangeSimple ToItem(ledgerstatechange entity)
//        {
//            return new LedgerStateChangeSimple(entity.ledger_height, entity.raw);
//        }

//        protected override DbSet<ledgerstatechange> GetDbSet(BlockchainEntities entities)
//        {
//            return entities.ledgerstatechanges;
//        }
//    }
//}
