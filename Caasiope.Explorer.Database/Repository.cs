using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using Caasiope.Explorer.Database.SQL;
using Helios.Common.Extensions;
using Helios.Common.Logs;
using Helios.Common.Synchronization;

namespace Caasiope.Explorer.Database
{
    // this class handles the cache and the sql for type T
    // this class is internal

    public interface IRepository<in TItem> where TItem : class
    {
        void CreateOrUpdate(ExplorerEntities entities, TItem item);
        string TableName { get; }
    }

    public abstract class Repository
    {
        protected ILogger logger;
        public void SetLogger(ILogger logger) { this.logger = logger; }
        public abstract void Initialize(ExplorerEntities entities);
    }

    public abstract class Repository<TItem> : Repository
    {
        protected class UnwrapEnumerator : IEnumerator<TItem>
        {
            private readonly IEnumerator<Wrapper> enumerator;

            public UnwrapEnumerator(IEnumerator<Wrapper> enumerator)
            {
                this.enumerator = enumerator;
            }

            public void Dispose()
            {
                enumerator.Dispose();
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }

            TItem IEnumerator<TItem>.Current => enumerator.Current.Item;

            public object Current => Current;
        }

        public class Wrapper
        {
            public TItem Item;
            public Wrapper(TItem item) { Item = item; }
        }

        protected abstract class Index
        {
            protected internal abstract void Add(Wrapper item);
        }

        protected abstract class PrimaryIndex : Index, IEnumerable<TItem>
        {
            // returns true if found
            protected internal abstract bool GetOrCreate(TItem item, out Wrapper wrapper);

            public abstract IEnumerator<TItem> GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public abstract void CreateOrUpdate(ExplorerEntities entities, TItem item);
        protected  abstract PrimaryIndex GetPrimaryIndex();

        // all the index but not the primary
        private readonly List<Index> indexes = new List<Index>();

        protected readonly MonitorLocker locker = new MonitorLocker();

        protected void RegisterIndex(Index index)
        {
            indexes.Add(index);
        }
        
        public IEnumerable<TItem> GetEnumerable()
        {
            using (locker.CreateLock())
            {
                return GetPrimaryIndex();
            }
        }

        protected bool TryUpdateCache(TItem item)
        {
            // get or create wrapper
            if (!GetPrimaryIndex().GetOrCreate(item, out var wrapper))
            {
                // and add into cache indexes
                AddIntoIndexes(wrapper);
                return true;
            }

            // update cache
            wrapper.Item = item;
            return false;
        }

        protected void AddIntoIndexes(Wrapper wrapper)
        {
            foreach (var index in indexes)
                index.Add(wrapper);
        }
    }

    public abstract class Repository<TItem, TEntity> : Repository<TItem>, IRepository<TItem> where TEntity : class where TItem : class
    {
        public override void Initialize(ExplorerEntities entities)
        {
            TableName = GetTableName(entities);

            Debug.Assert(!GetEnumerable().Any());

            using (new PerformanceLogger(logger, $"Initialize {GetType().Name}"))
            {
                foreach (var item in GetInitial(entities))
                {
                    var wrapper = new Wrapper(item);
                    GetPrimaryIndex().Add(wrapper);
                    AddIntoIndexes(wrapper);
                }
            }
        }

        public override void CreateOrUpdate(ExplorerEntities entities, TItem item)
        {
            using (locker.CreateLock())
            {
                var isNew = TryUpdateCache(item);
                var entity = ToEntity(item);

                if (!isNew)
                {
                    GetDbSet(entities).Attach(entity);
                    ((IObjectContextAdapter) entities).ObjectContext.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);
                }
                else
                {
                    GetDbSet(entities).Add(entity);
                }
            }
        }

        public string TableName { get; private set; }

        private IEnumerable<TItem> GetInitial(ExplorerEntities entities)
        {
            return GetDbSet(entities).Select(ToItem);
        }

        private string GetTableName(ExplorerEntities entities)
        {
            var objectContext = ((IObjectContextAdapter)entities).ObjectContext;
            return  objectContext.CreateObjectSet<TEntity>().EntitySet.Name;
        }

        protected abstract DbSet<TEntity> GetDbSet(ExplorerEntities entities);

        protected abstract TEntity ToEntity(TItem item);
        protected abstract TItem ToItem(TEntity entity);
    }

    public abstract class Repository<TItem, TEntity, TKey> : Repository<TItem, TEntity> where TEntity : class where TItem : class
    {
        private class KeyPrimaryIndex : PrimaryIndex
        {
            private readonly Func<TItem, TKey> getKey;
            private readonly Dictionary<TKey, Wrapper> cache = new Dictionary<TKey, Wrapper>();

            public KeyPrimaryIndex(Func<TItem, TKey> getKey)
            {
                this.getKey = getKey;
            }

            protected internal override void Add(Wrapper item)
            {
                cache.Add(getKey(item.Item), item);;
            }

            protected internal override bool GetOrCreate(TItem item, out Wrapper wrapper)
            {
                var isNew = false;
                var key = getKey(item);
                wrapper = cache.GetOrCreate(key, k =>
                {
                    isNew = true;
                    return new Wrapper(item);
                });
                return !isNew;
            }

            public override IEnumerator<TItem> GetEnumerator()
            {
                return new UnwrapEnumerator(cache.Values.GetEnumerator());
            }

            public TItem GetByKey(TKey key)
            {
                return cache.TryGetValue(key, out var wrapper) ? wrapper.Item : null;
            }
        }

        private readonly KeyPrimaryIndex primary;

        protected Repository()
        {
            this.primary = new KeyPrimaryIndex(GetKey);
        }

        protected override PrimaryIndex GetPrimaryIndex()
        {
            return primary;
        }

        protected abstract TKey GetKey(TItem item);

        public TItem GetByKey(TKey key)
        {
            using (locker.CreateLock())
            {
                return primary.GetByKey(key);
            }
        }
    }
    
    public class EnumeratorToEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerator<T> enumerator;

        public EnumeratorToEnumerable(IEnumerator<T> enumerator)
        {
            this.enumerator = enumerator;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
