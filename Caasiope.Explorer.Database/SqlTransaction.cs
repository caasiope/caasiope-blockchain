using System;
using Caasiope.Explorer.Database.Managers;
using Caasiope.Explorer.Database.SQL;
using Helios.Common.Extensions;
using Helios.Common.Logs;

namespace Caasiope.Explorer.Database
{
    public abstract class SqlTransaction
    {
        private ILogger logger;

        public void Initialize(ILogger logger)
        {
            this.logger = logger;
        }

        protected abstract void Populate(RepositoryManager repositories, BlockchainEntities entities);

        public void Save(RepositoryManager repositories)
        {
            using (var entities = new BlockchainEntities())
            {
                using (new PerformanceLogger(logger, "PopulateEntitiesLogger"))
                {
                    // TODO this is slow
                    Populate(repositories, entities);
                }
                using (new PerformanceLogger(logger, "SaveDatabaseLogger"))
                {
                    entities.SaveChanges();
                }
            }
            OnSaveFinished();
        }

        protected abstract void OnSaveFinished();
    }
    public abstract class SqlTransaction<T> : SqlTransaction
    {
        protected readonly T Data;

        public Action<T> Callback { get; set; }

        protected SqlTransaction(T data)
        {
            Data = data;
        }

        protected sealed override void OnSaveFinished()
        {
            Callback.Call(Data);
        }
    }
}
