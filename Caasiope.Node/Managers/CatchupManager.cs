using Caasiope.JSON.Helpers;
using Caasiope.Node.Connections;
using Caasiope.Node.Services;
using Helios.Common.Logs;

namespace Caasiope.Node.Managers
{
    public class CatchupManager
    {
        [Injected] public IConnectionService ConnectionService;
        [Injected] public ILedgerService LedgerService;

        private long targetHeight;
        private long? preventHeight;
        private ILogger logger;

        public void Initialize(ILogger logger)
        {
            Injector.Inject(this);
            this.logger = logger;
        }

        // TODO Change name
        // after we receive a ledger and we catch up
        public bool TryProcessHeight(IConnectionSession session)
        {
            return TryProcessIncomingHeight(session, targetHeight);
        }

        // TODO Change name
        // we need to see if an incoming height is ahead of us
        public bool TryProcessIncomingHeight(IConnectionSession session, long incoming)
        {
            return TryRequest(session, incoming, LedgerService.LedgerManager.GetNextHeight());
        }

        private bool TryRequest(IConnectionSession session, long incoming, long next)
        {
            if (preventHeight.HasValue && preventHeight.Value == next)
                return false;

            // if incoming height is next or greater than next then request next
            if (next <= incoming)
            {
                targetHeight = incoming;
                ConnectionService.BlockchainChannel.Send(session, RequestHelper.CreateGetSignedLedgerRequest(next));
                logger.Log($"CatchupManager : Requesting [{next} of {incoming}] ");
                preventHeight = next;
                return true;
            }
            return false;
        }
    }
}