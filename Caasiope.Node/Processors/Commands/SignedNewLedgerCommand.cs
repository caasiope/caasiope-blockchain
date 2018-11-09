using Caasiope.Node.Connections;
using Caasiope.Protocol.Types;
using Caasiope.Protocol.Validators;

namespace Caasiope.Node.Processors.Commands
{
    public class SignedNewLedgerCommand : LiveCommand<SignedNewLedger>
    {
        private readonly IConnectionSession session;
        public SignedNewLedgerCommand(SignedNewLedger data, IConnectionSession session) : base(data)
        {
            this.session = session;
        }

        protected override ResultCode GetResult(SignedNewLedger signed)
        {
            // TODO Check PreviousLedgerHash!
            var status = LedgerService.LedgerManager.SignedLedgerValidator.Validate(signed);
            if (status != LedgerValidationStatus.Ok)
            {
                Logger.LogDebug($"SignedNewLedgerNotification. Validation failed : {status}");
                return ResultCode.Failed;
            }

            LiveService.CatchupManager.TryProcessIncomingHeight(session, signed.Height);

            return ResultCode.Success;
        }
    }
}