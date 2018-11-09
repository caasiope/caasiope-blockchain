using System;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class AddDeclaration : InjectedConsoleCommand
    {
        private readonly CommandArgument aliasArgument;

        public AddDeclaration() : base("adddeclaration")
        {
            aliasArgument = RegisterArgument(new CommandArgument("alias"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            TxDeclaration declaration;
            if (!WalletService.TryGetDeclaration(aliasArgument.Value, out declaration))
            {
                Console.WriteLine("Declaration Not Found!");
                return;
            }

            WalletService.PrepareTransactionManager.AddDeclaration(declaration);
            Console.WriteLine("Declaration Added !");
        }
    }
}