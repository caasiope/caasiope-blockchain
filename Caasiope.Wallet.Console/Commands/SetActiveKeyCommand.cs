using System;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class SetActiveKeyCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument aliasArgument;

        // setactivekey btcissuer
        public SetActiveKeyCommand() : base("setactivekey")
        {
            aliasArgument = RegisterArgument(new CommandArgument("alias"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var alias = aliasArgument.Value;
            if(WalletService.SetActiveKey(alias))
                Console.WriteLine($"Successfully set active key to : {alias}");
        }
    }
}