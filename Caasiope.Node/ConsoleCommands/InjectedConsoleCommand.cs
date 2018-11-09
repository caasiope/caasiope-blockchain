using Caasiope.Node.Services;
using Helios.Common;

namespace Caasiope.Node.ConsoleCommands
{
    public abstract class InjectedConsoleCommand : ConsoleCommand
    {
        [Injected] public ILiveService LiveService;
        [Injected] public ILedgerService LedgerService;
        [Injected] public IConnectionService ConnectionService;
        [Injected] public ConsoleCommandProcessor ConsoleCommandProcessor;

        protected InjectedConsoleCommand(string name) : base(name)
        {
            Injector.Inject(this);
        }

        protected InjectedConsoleCommand()
        {
            Injector.Inject(this);
        }
    }
}