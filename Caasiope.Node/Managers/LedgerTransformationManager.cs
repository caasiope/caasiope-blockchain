using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Node.Services;
using Caasiope.Node.Transformers;
using Caasiope.Protocol.Types;
using Helios.Common.Logs;
using Helios.Common.Synchronization;

namespace Caasiope.Node.Managers
{
    // store Table Name CurrentHeight TargetHeight - it is a wrapper
    // Register Transaformer callback
    // Update CurrentHeight on callback
    // After Update check is everyone is updated and signal finished

    // TODO Full of bugs!
    // This class listen to new ledgers and handles consistency of SQL db
    internal class LedgerTransformationManager
    {
        [Injected] public IDatabaseService DatabaseService;
        [Injected] public IDataTransformationService DataTransformationService;
        
        private readonly Dictionary<string, TableTransformationState> tableTransformationStates = new Dictionary<string, TableTransformationState>();
        private readonly MonitorLocker locker = new MonitorLocker();
        private readonly AutoResetEvent finished = new AutoResetEvent(false);
        private long current;
        private ILogger logger;

        public void Initialize(ILogger logger)
        {
            Injector.Inject(this);
            this.logger = logger;
        }

        public void Start()
        {
            // cannot get it before
            // Update cache by latest data from height tables in the SQl db
            var lastLedger = DatabaseService.ReadDatabaseManager.GetLastLedgerFromRaw();
            current = lastLedger?.Ledger.LedgerLight.Height ?? -1;

            SetInitialTableHeights(current);

            // in case we are already on the target height
            if (IsFinished())
                finished.Set();
        }

        private void SetInitialTableHeights(long targetHeight)
        {
            SetInitialTableHeights();

            // determine which height still need to be transformed
            var minimal = GetMinimalHeight(tableTransformationStates.Values);

            var min = tableTransformationStates.Values.Min(table => table.CurrentHeight);

            if (targetHeight == min)
                return;

            //TODO use batch
            var ledgers = DatabaseService.ReadDatabaseManager.GetLedgersFromHeight(minimal).ToDictionary(_ => _.Ledger.Ledger.LedgerLight.Height);

            var knownAddresses = DatabaseService.ReadDatabaseManager.GetAddresses();

            for (var height = min + 1; height <= targetHeight; height++)
            {
                Debug.Assert(ledgers.ContainsKey(height));
                var context = new DataTransformationContext(ledgers[height]);
                
                foreach (var entity in tableTransformationStates.Values)
                {
                    if (entity.TableName == "accounts")
                    {
                        context = RebuildContext(context, knownAddresses);
                    }
                    if(entity.CurrentHeight < height)
                        TransformLedgerState(context, entity.TableName, height);
                }
            }
        }

        private DataTransformationContext RebuildContext(DataTransformationContext context, HashSet<Address> knownAccounts)
        {
            var oldState = context.SignedLedgerState.State;
            var accounts = MarkNewAccounts(knownAccounts, oldState.Accounts);
            var state = new LedgerStateChange(accounts, oldState.MultiSignatures, oldState.HashLocks, oldState.TimeLocks, oldState.VendingMachines);
            var signedLedgerState = new SignedLedgerState(context.SignedLedgerState.Ledger, state);
            return new DataTransformationContext(signedLedgerState);
        }

        private static List<Account> MarkNewAccounts(HashSet<Address> knownAccounts, List<Account> accounts)
        {
            var results = new List<Account>();

            foreach (var account in accounts)
            {
                if (knownAccounts.Add(account.Address))
                {
                    var mutable = new MutableAccount(account.Address, account.CurrentLedger);
                    results.Add(mutable.SetBalances(account.Balances).SetDeclaration(account.Declaration));
                }
                else
                {
                    results.Add(account);
                }
            }

            return results;
        }

        private void SetInitialTableHeights()
        {
            var heightsFromDatabase = DatabaseService.ReadDatabaseManager.GetHeightTables();
            var initials = DataTransformationService.DataTransformerManager.GetInitialTableHeights();
            // This is in case of the cold start, or if an new table added so it will be indexed in heightTables
            foreach (var initialEntity in initials)
            {
                // set callback
                DataTransformationService.DataTransformerManager.RegisterOnProcessed(initialEntity, OnTransformerFinished);

                var entity = (heightsFromDatabase.Contains(initialEntity)) ? heightsFromDatabase.Find(_ => _.TableName == initialEntity.TableName) : initialEntity;
                tableTransformationStates.Add(entity.TableName, new TableTransformationState(entity, entity.Height));
            }

            Debug.Assert(tableTransformationStates.Count == initials.Count); // we assert that we didn't lose anything
        }

        public void OnTransformerFinished(string tablename, long height)
        {
            using (locker.CreateLock())
            {
                var table = tableTransformationStates[tablename];
                table.CurrentHeight = height;

                if (IsFinished())
                {
                    logger.Log($"Transformation Finished. Height: {height}");
                    finished.Set();
                }
            }
        }

        public void WaitFinished()
        {
            finished.WaitOne();
        }

        private bool IsFinished()
        {
            var currentHeight = current;

            foreach (var state in tableTransformationStates.Values)
            {
                // Debug.Assert(currentHeight == state.TargetHeight); // TODO find why it doesnt work
                if (state.CurrentHeight != currentHeight)
                    return false;
            }

            return true;
        }

        // Called on run
        public void ProcessLedger(SignedLedgerState ledger)
        {
            var target = ledger.Ledger.Ledger.LedgerLight.Height;
            var context = new DataTransformationContext(ledger);
            foreach (var table in tableTransformationStates.Values)
            {
                Debug.Assert(table.TargetHeight + 1 == target);
                TransformLedgerState(context, table.TableName, target);
            }
            current = target;
        }
        
        private void TransformLedgerState(DataTransformationContext context, string table, long height)
        {
            tableTransformationStates[table].TargetHeight = context.SignedLedgerState.Ledger.Ledger.LedgerLight.Height;
            DataTransformationService.DataTransformerManager.Transform(context, new TableLedgerHeight(table, height));
        }

        private long GetMinimalHeight(IEnumerable<TableTransformationState> tables)
        {
            return tables.Min(_ => _.TargetHeight);
        }
        
        private class TableTransformationState
        {
            public readonly string TableName;
            public long CurrentHeight;
            public long TargetHeight;

            public TableTransformationState(TableLedgerHeight height, long targetHeight)
            {
                TableName = height.TableName;
                CurrentHeight = height.Height;
                TargetHeight = targetHeight;
            }
        }
    }
}