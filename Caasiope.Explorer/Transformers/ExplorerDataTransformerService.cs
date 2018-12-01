using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Caasiope.Explorer.Database;
using Caasiope.Explorer.Database.Managers;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Services;
using Caasiope.Log;
using Caasiope.Node;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Services;
using Helios.Common.Extensions;
using Helios.Common.Logs;
using ThreadedService = Helios.Common.Concepts.Services.ThreadedService;

namespace Caasiope.Explorer.Transformers
{
    internal interface IDataTransformerService : IService
    {
        void ProcessNext(DataTransformationContext context);
        void RegisterOnProcessed(Action<string, long> callback);
        string TableName { get; }
    }

    internal abstract class DataTransformerService : ThreadedService { }

    internal abstract class ExplorerDataTransformerService<TItem, TRepository> : DataTransformerService, IDataTransformerService where TItem : class where TRepository : Repository<TItem>, Caasiope.Explorer.Database.IRepository<TItem>
    {
        [Injected] public IExplorerDatabaseService DatabaseService;

        public override ILogger Logger { get; }

        private readonly ConcurrentQueue<DataTransformationContext> queue = new ConcurrentQueue<DataTransformationContext>();

        protected ExplorerDataTransformerService()
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
            var transaction = new Database.SqlTransactions.TransformerSqlTransaction<TItem>(Transform(context), Repository, context.SignedLedgerState.Ledger.Ledger.LedgerLight.Height, Logger);
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


    public class TransactionDeclarationContext
    {
        public readonly Dictionary<Address, TransactionDeclarationEntity> AddressDeclarations = new Dictionary<Address, TransactionDeclarationEntity>();
        public readonly List<TransactionDeclarationEntity> Declarations = new List<TransactionDeclarationEntity>();

        public void TryAdd(TransactionDeclarationEntity entity, TxDeclaration declaration)
        {
            // we can duplicate declarations like Secret Reveliation
            Declarations.Add(entity);

            // we include unly once Account declarations
            if (declaration is TxAddressDeclaration)
            {
                var address = ((TxAddressDeclaration) declaration).Address;
                if (!AddressDeclarations.ContainsKey(address))
                    AddressDeclarations.Add(address, entity);
            }
        }
    }

    public class DataTransformationContext
    {
        private readonly ManualResetEvent declarationCreated = new ManualResetEvent(false);
        private TransactionDeclarationContext declarations;
        public readonly SignedLedgerState SignedLedgerState;

        public DataTransformationContext(SignedLedgerState signedLedgerState)
        {
            SignedLedgerState = signedLedgerState;
        }

        public void SetDeclarations(TransactionDeclarationContext declarations)
        {
            this.declarations = declarations;
            declarationCreated.Set();
        }

        public TransactionDeclarationContext GetDeclarations()
        {
            declarationCreated.WaitOne();
            return declarations;
        }
    }
}
