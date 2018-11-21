using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.Database.Managers;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Helios.Common.Logs;

namespace Caasiope.Explorer.Database.SqlTransactions
{
    public class TransformerSqlTransaction<TItem> : SqlTransaction<IEnumerable<TItem>> where TItem : class
    {
        private readonly IRepository<TItem> repository;
        private readonly long height;
        private readonly ILogger logger;

        public TransformerSqlTransaction(IEnumerable<TItem> data, IRepository<TItem> repository, long height, ILogger logger) : base(data)
        {
            this.repository = repository;
            this.height = height;
            this.logger = logger;
        }

        protected override void Populate(RepositoryManager repositories, BlockchainEntities entities)
        {
            // get transformed data
            foreach (var entity in Data)
            {
                repository.CreateOrUpdate(entities, entity);
            }

            var heights = repositories.GetRepository<TableLedgerHeightRepository>();
            var heightTable = new TableLedgerHeight(repository.TableName, height);
            heights.CreateOrUpdate(entities, heightTable);

            logger.Log($"{repository.TableName}: {Data.Count()}, Height: {height}");
        }
    }
}
