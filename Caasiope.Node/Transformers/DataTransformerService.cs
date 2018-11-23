using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Caasiope.Database;
using Caasiope.Database.Managers;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SqlTransactions;
using Caasiope.Log;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Services;
using Helios.Common.Extensions;
using Helios.Common.Logs;
using ThreadedService = Helios.Common.Concepts.Services.ThreadedService;

namespace Caasiope.Node.Transformers
{
    internal interface IDataTransformerService : IService
    {
        void ProcessNext(DataTransformationContext context);
        void RegisterOnProcessed(Action<string, long> callback);
        string TableName { get; }
    }

    internal abstract class DataTransformerService : ThreadedService { }

    internal abstract class DataTransformerService<TItem, TRepository> : DataTransformerService, IDataTransformerService where TItem : class where TRepository : Repository, IRepository<TItem>
    {
        [Injected] public IDatabaseService DatabaseService;

        public override ILogger Logger { get; }

        private readonly ConcurrentQueue<DataTransformationContext> queue = new ConcurrentQueue<DataTransformationContext>();

        protected DataTransformerService()
        {
            Logger = new LoggerAdapter(nameof(DataTransformerService)); // TODO may be change to Name
        }

        protected TRepository Repository;
        private Action<string, long> ledgerProcessed;

        public void RegisterOnProcessed(Action<string, long> callback)
        {
            ledgerProcessed += callback;
        }
        protected override void OnInitialize()
        {
            Repository = GetRepository(DatabaseService.RepositoryManager);
        }

        private TRepository GetRepository(RepositoryManager repositories)
        {
            return repositories.GetRepository<TRepository>();
        }

        protected override void OnStart() { }

        protected override void OnStop() { }

        protected override void Run()
        {
            if (TryProcess())
                trigger.Set();
        }

        private bool TryProcess()
        {
            if (!queue.TryDequeue(out var context))
                return false;

            ProcessSave(context);
            ledgerProcessed.Call(TableName, context.SignedLedgerState.Ledger.Ledger.LedgerLight.Height);

            return true;
        }

        private void ProcessSave(DataTransformationContext context)
        {
            var transaction = new TransformerTransaction<TItem>(Transform(context), Repository, context.SignedLedgerState.Ledger.Ledger.LedgerLight.Height, Logger);
            DatabaseService.SqlTransactionManager.ExecuteTransaction(transaction);
        }

        protected abstract IEnumerable<TItem> Transform(DataTransformationContext context);

        // this is avaliable only after initialization
        public string TableName => Repository.TableName;

        public void ProcessNext(DataTransformationContext context)
        {
            queue.Enqueue(context);
            trigger.Set();
        }
    }
    
    public class DataTransformationContext
    {
        public readonly SignedLedgerState SignedLedgerState;

        public DataTransformationContext(SignedLedgerState signedLedgerState)
        {
            SignedLedgerState = signedLedgerState;
        }
    }
}
