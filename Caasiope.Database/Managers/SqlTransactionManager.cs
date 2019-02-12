using System.Collections.Concurrent;
using System.Threading;
using Helios.Common.Logs;

namespace Caasiope.Database.Managers
{
    public class SqlTransactionManager
    {
        private readonly AutoResetEvent trigger;
        private readonly ConcurrentQueue<Transaction> queue = new ConcurrentQueue<Transaction>();
        private readonly RepositoryManager repositories;
        private readonly ILogger logger;

        public SqlTransactionManager(AutoResetEvent trigger, RepositoryManager repositories, ILogger logger)
        {
            this.trigger = trigger;
            this.repositories = repositories;
            this.logger = logger;
        }

        // set transaction to be processed later
        public void Save(Transaction transaction)
        {
            queue.Enqueue(transaction);
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
        public void ExecuteTransaction(Transaction transaction)
        {
            transaction.Initialize(logger);
            transaction.Save(repositories);
        }
    }
}