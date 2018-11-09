using System.Threading;
using Caasiope.Database.Managers;
using Helios.Common.Concepts.Services;
using Helios.Common.Logs;

namespace Caasiope.Node.Services
{
    public class DatabaseService : ThreadedService, IDatabaseService
    {
        private readonly AutoResetEvent saveCompleted = new AutoResetEvent(false);

        public bool IsSave { get; set; }

        public ReadDatabaseManager ReadDatabaseManager { get; }
        public RepositoryManager RepositoryManager { get; }
        public SqlTransactionManager SqlTransactionManager { get; }

        public DatabaseService()
        {
            IsSave = true;
            RepositoryManager = new RepositoryManager(Logger);
            SqlTransactionManager = new SqlTransactionManager(trigger, RepositoryManager, Logger);
            ReadDatabaseManager = new ReadDatabaseManager(RepositoryManager);
        }

        protected override void OnInitialize()
        {
            RepositoryManager.Initialize();
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }
        
        public void WaitSaveCompleted()
        {
            if (!IsRunning)
                return;

            saveCompleted.WaitOne();
        }

        protected override void Run()
        {
            if (!IsSave)
                return;

            saveCompleted.Reset();

            while (SqlTransactionManager.ProcessOne()) { }

            saveCompleted.Set();
        }
    }

    public interface IDatabaseService : IService
    {
        ReadDatabaseManager ReadDatabaseManager { get; }
        SqlTransactionManager SqlTransactionManager { get; }
        RepositoryManager RepositoryManager { get; }

        bool IsSave { get; set; }

        void WaitSaveCompleted();
    }
}
