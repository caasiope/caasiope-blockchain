using Caasiope.Node;
using Caasiope.Wallet.Services;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public abstract class InjectedConsoleCommand : Node.ConsoleCommands.InjectedConsoleCommand
    {
        [Injected] public IWalletService WalletService;

        protected InjectedConsoleCommand(string name) : base(name) { }

        protected InjectedConsoleCommand() : base() { }
    }
}
