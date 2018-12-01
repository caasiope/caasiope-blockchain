using Caasiope.Log;
using Helios.Common.Concepts.Services;
using Helios.Common.Logs;
using SqlTransactionManager = Caasiope.Explorer.Database.Managers.SqlTransactionManager;

namespace Caasiope.Explorer.Services
{
    public interface IExplorerDatabaseService : IService
    {
        Database.Managers.ReadDatabaseManager ReadDatabaseManager { get; }
        Database.Managers.RepositoryManager RepositoryManager { get; }
        SqlTransactionManager SqlTransactionManager { get; set; }
    }

    public class ExplorerDatabaseService : ThreadedService,  IExplorerDatabaseService
    {
        public Database.Managers.ReadDatabaseManager ReadDatabaseManager { get; set; }
        public Database.Managers.RepositoryManager RepositoryManager { get; }
        public SqlTransactionManager SqlTransactionManager { get; set; }

        public ExplorerDatabaseService()
        {
            Logger = new LoggerAdapter(Name);
            RepositoryManager = new Database.Managers.RepositoryManager(Logger);
            SqlTransactionManager = new SqlTransactionManager(trigger, RepositoryManager, Logger);
            ReadDatabaseManager = new Database.Managers.ReadDatabaseManager(RepositoryManager);
        }

        public override ILogger Logger { get; }

        protected override void OnInitialize()
        {
            RepositoryManager.Initialize();
        }

        protected override void OnStart() { }

        protected override void OnStop() { }

        public void WaitSaveCompleted()
        {
            if (!IsRunning)
                return;

            //saveCompleted.WaitOne();
        }

        protected override void Run()
        {
           // if (!IsSave)
           //     return;

            //saveCompleted.Reset();

            while (SqlTransactionManager.ProcessOne()) { }

            //saveCompleted.Set();
        }
    }
}