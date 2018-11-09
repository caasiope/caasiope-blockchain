using System;
using Caasiope.Database.SqlTransactions;
using Caasiope.Protocol.Types;
using Caasiope.Protocol;

namespace Caasiope.Node.Services
{
    public class PersistenceManager
    {
        [Injected] public IDatabaseService DatabaseService;
        [Injected] public IDataTransformationService DataTransformationService;

        public void Initialize()
        {
            Injector.Inject(this);
        }

        public void Save(SignedLedgerState state)
        {
            // ConsoleLogger.Instance.Log("Save Ledger", null);
            DatabaseService.SqlTransactionManager.Save(new SignedLedgerSqlTransaction(state) { Callback = DataTransformationService.Transform });
        }
    }
}