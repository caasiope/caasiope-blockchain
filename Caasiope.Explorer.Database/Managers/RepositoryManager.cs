using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Explorer.Database.SQL;
using Helios.Common.Logs;

namespace Caasiope.Explorer.Database.Managers
{
    // this class is managing creation and access to repositories
    public class RepositoryManager
    {
        readonly Dictionary<Type, Repository> repositories = new Dictionary<Type, Repository>();
        
        public RepositoryManager(ILogger logger)
        {
            CreateInstances(logger);
        }

        // we create and register a single instance of each type of repository
        private void CreateInstances(ILogger logger)
        {
            foreach (var repoType in GetAllRepoTypes())
                AddRepository(repoType, logger);
        }

        private void AddRepository(Type repoType, ILogger logger)
        {
            repositories.Add(repoType, CreateInstance(repoType, logger));
        }

        private static Repository CreateInstance(Type repoType, ILogger logger)
        {
            var repository = (Repository) Activator.CreateInstance(repoType);
            repository.SetLogger(logger);
            return repository;
        }

        // we use reflection to dynamicly load every repository type
        public static Type[] GetAllRepoTypes()
        {
            return typeof(RepositoryManager).Assembly
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract
                && t.Namespace == typeof(BalanceRepository).Namespace
                && (t.IsSubclassOf(typeof(Repository)))).ToArray();
        }

        // we initialize every repository and load data from database
        public void Initialize()
        {
            using (var entities = new ExplorerEntities())
            {
                foreach (var repository in repositories.Values)
                    repository.Initialize(entities);
            }
        }

        public T GetRepository<T>() where T : Repository
        {
            return (T)repositories[typeof(T)];
        }
    }
}
