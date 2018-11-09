using System;
using System.Linq;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Chronicles;

namespace Caasiope.Node.Sagas
{
    class FinalizeLedgerSaga : UpdateStateSaga<FinalizeLedgerFolklore>
    {
        protected override void Initialize(FinalizeLedgerFolklore folklore)
        {
            throw new NotImplementedException();
        }

        protected override void Terminate(FinalizeLedgerFolklore folklore)
        {
            LiveService.PersistenceManager.Save(new SignedLedgerState(folklore.SignedLedger, GetStateChange()));
        }
    }

    internal class FinalizeLedgerFolklore
    {

        public FinalizeLedgerFolklore(SignedLedger signedLedger)
        {
            SignedLedger = signedLedger;
        }

        public SignedLedger SignedLedger { get; }
    }

    class FinalizeLedgerBard : Bard<FinalizeLedgerFolklore, FinalizeLedgerSaga>
    {
        public FinalizeLedgerBard(FinalizeLedgerFolklore folklore, FinalizeLedgerSaga saga) : base(folklore, saga) { }
    }
}
