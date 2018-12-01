using System.Collections.Generic;
using System.Data.Entity;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Explorer.Database.SQL.Entities;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Explorer.Database.Repositories
{
    public class TransactionRepository : Repository<SignedTransactionSimple, transaction, TransactionHash>
    {
        private readonly TransactionIndex transactions = new TransactionIndex();

        // transactions by height
        private class TransactionIndex : Index
        {
            private readonly Dictionary<long, List<Wrapper>> cache = new Dictionary<long, List<Wrapper>>();

            protected internal override void Add(Wrapper item)
            {
                cache.GetOrCreate(item.Item.LedgerHeight).Add(item);
            }

            public List<Wrapper> GetByHeight(long height)
            {
                List<Wrapper> list;
                if (!cache.TryGetValue(height, out list))
                    return new List<Wrapper>();
                return list;
            }
        }

        protected override DbSet<transaction> GetDbSet(ExplorerEntities entities)
        {
            return entities.transactions;
        }

        protected override TransactionHash GetKey(SignedTransactionSimple item)
        {
            return item.TransactionHash;
        }

        protected override transaction ToEntity(SignedTransactionSimple item)
        {
            return new transaction
            {
                hash = item.TransactionHash.Bytes,
                ledger_height = item.LedgerHeight,
                ledger_timestamp = item.LedgerTimestamp,
                expire = item.Expire
            };
        }

        protected override SignedTransactionSimple ToItem(transaction entity)
        {
            return new SignedTransactionSimple(new TransactionHash(entity.hash), entity.ledger_height, entity.expire, entity.ledger_timestamp);
        }

        public IEnumerable<SignedTransactionSimple> GetByHeight(long height)
        {
            return new EnumeratorToEnumerable<SignedTransactionSimple>(new UnwrapEnumerator(transactions.GetByHeight(height).GetEnumerator()));
        }
    }
}
