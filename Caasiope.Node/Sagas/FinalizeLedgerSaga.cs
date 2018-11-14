using System;
using System.Linq;
using Caasiope.Node.Types;
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
        }
    }

    internal class FinalizeLedgerFolklore
    {

        public FinalizeLedgerFolklore(MutableLedgerState state)
        {
            LedgerState = state;
        }

        public MutableLedgerState LedgerState { get; }
    }

    class FinalizeLedgerBard : Bard<FinalizeLedgerFolklore, FinalizeLedgerSaga>
    {
        public FinalizeLedgerBard(FinalizeLedgerFolklore folklore, FinalizeLedgerSaga saga) : base(folklore, saga) { }
    }
}
