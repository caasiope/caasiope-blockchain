using System.Collections.Concurrent;
using System.Threading;
using Helios.Common.Logs;

namespace Caasiope.Explorer.Database.Managers
{
    public class SqlTransactionManager
    {
        private readonly AutoResetEvent trigger;
        private readonly ConcurrentQueue<SqlTransaction> queue = new ConcurrentQueue<SqlTransaction>();
        private readonly RepositoryManager repositories;
        private readonly ILogger logger;

        public SqlTransactionManager(AutoResetEvent trigger, RepositoryManager repositories, ILogger logger)
        {
            this.trigger = trigger;
            this.repositories = repositories;
            this.logger = logger;
        }

        // set transaction to be processed later
        public void Save(SqlTransaction sqlTransaction)
        {
            queue.Enqueue(sqlTransaction);
            trigger.Set();
        }

        // synchronously executes the first sql transaction
        public bool ProcessOne()
        {
            if (queue.TryDequeue(out var transaction))
            {
                ExecuteTransaction(transaction);
                return true;
            }
            return false;
        }

        // synchronously executes the sql transaction
        public void ExecuteTransaction(SqlTransaction sqlTransaction)
        {
            sqlTransaction.Initialize(logger);
            sqlTransaction.Save(repositories);
        }
    }
}
