using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Caasiope.Database.SQL;
using Helios.Common.Logs;

namespace Caasiope.Database
{
    // this class handles the cache and the sql for type T
    // this class is internal

    public interface IRepository<in TItem> where TItem : class
    {
        void CreateOrUpdate(BlockchainEntities entities, TItem item);
        string TableName { get; }
    }

    public abstract class Repository
    {
        protected ILogger logger;
        public void SetLogger(ILogger logger) { this.logger = logger; }
        public abstract void Initialize(BlockchainEntities entities);
    }

    public abstract class Repository<TItem, TEntity> : Repository, IRepository<TItem> where TEntity : class where TItem : class
    {
        public override void Initialize(BlockchainEntities entities)
        {
            TableName = GetTableName(entities);
        }

        public void CreateOrUpdate(BlockchainEntities entities, TItem item)
        {
            var entity = ToEntity(item);
            var isNew = CheckIsNew(entities, entity);

            if (!isNew)
            {
                GetDbSet(entities).Attach(entity);
                ((IObjectContextAdapter)entities).ObjectContext.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);
            }
            else
            {
                GetDbSet(entities).Add(entity);
            }
        }

        protected abstract bool CheckIsNew(BlockchainEntities entities, TEntity entity);

        public string TableName { get; private set; }

        public IEnumerable<TItem> GetEnumerable()
        {
            using (var entities = new BlockchainEntities())
            {
                return GetDbSet(entities).Select(ToItem).ToList();
            }
        }

        private string GetTableName(BlockchainEntities entities)
        {
            var objectContext = ((IObjectContextAdapter)entities).ObjectContext;
            return objectContext.CreateObjectSet<TEntity>().EntitySet.Name;
        }

        protected abstract DbSet<TEntity> GetDbSet(BlockchainEntities entities);

        protected abstract TEntity ToEntity(TItem item);
        protected abstract TItem ToItem(TEntity entity);
    }
}
