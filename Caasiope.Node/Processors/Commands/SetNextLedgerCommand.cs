using System;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Processors.Commands
{
    public class SetNextLedgerCommand : LedgerCommand<SignedLedger>
    {
        private readonly Action onFinish;

        public SetNextLedgerCommand(SignedLedger data, Action onFinish) : base(data)
        {
            this.onFinish = onFinish;
        }

        protected override void DoWork(SignedLedger input)
        {
            LedgerService.LedgerManager.SetNextLedger(input, onFinish);
        }
    }
}