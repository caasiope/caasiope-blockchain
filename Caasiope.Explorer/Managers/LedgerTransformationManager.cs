using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Services;
using Caasiope.Explorer.Transformers;
using Caasiope.Node;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Logs;
using Helios.Common.Synchronization;

namespace Caasiope.Explorer.Managers
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
        [Injected] public IExplorerDataTransformationService ExplorerDataTransformationService;
        [Injected] public IExplorerDatabaseService ExplorerDatabaseService;

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

            //TODO use batch
            var ledgers = DatabaseService.ReadDatabaseManager.GetLedgersFromHeight(minimal).ToDictionary(_ => _.Ledger.Ledger.LedgerLight.Height);

            var min = tableTransformationStates.Values.Min(table => table.CurrentHeight);

            for (var height = min + 1; height <= targetHeight; height++)
            {
                Debug.Assert(ledgers.ContainsKey(height));
                var context = new DataTransformationContext(ledgers[height]);

                foreach (var entity in tableTransformationStates.Values)
                {
                    if (entity.CurrentHeight < height)
                        TransformLedgerState(context, entity.TableName, height);
                }
            }
        }

        private void SetInitialTableHeights()
        {
            var heightsFromDatabase = ExplorerDatabaseService.ReadDatabaseManager.GetHeightTables();
            var initials = ExplorerDataTransformationService.DataTransformerManager.GetInitialTableHeights();
            // This is in case of the cold start, or if an new table added so it will be indexed in heightTables
            foreach (var initialEntity in initials)
            {
                // set callback
                ExplorerDataTransformationService.DataTransformerManager.RegisterOnProcessed(initialEntity, OnTransformerFinished);

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
                if(table.TargetHeight + 1 > target)
                    continue;
                TransformLedgerState(context, table.TableName, target);
            }
            current = target;
        }

        private void TransformLedgerState(DataTransformationContext context, string table, long height)
        {
            tableTransformationStates[table].TargetHeight = context.SignedLedgerState.Ledger.Ledger.LedgerLight.Height;
            ExplorerDataTransformationService.DataTransformerManager.Transform(context, new TableLedgerHeight(table, height));
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