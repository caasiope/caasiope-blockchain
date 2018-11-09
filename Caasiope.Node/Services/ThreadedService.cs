using Caasiope.Log;
using Helios.Common.Logs;

namespace Caasiope.Node.Services
{
    public abstract class ThreadedService : Helios.Common.Concepts.Services.ThreadedService
    {
		[Injected] public ILiveService LiveService;
	    [Injected] public IDatabaseService DatabaseService;
	    [Injected] public IConnectionService ConnectionService;
	    [Injected] public ILedgerService LedgerService;
	    [Injected] public IDataTransformationService DataTransformationService;

        public sealed override ILogger Logger { get; }

        protected ThreadedService()
        {
            Logger = new LoggerAdapter(Name);
        }
    }
}