using System;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class ListPrivateKeysCommand : InjectedConsoleCommand
    {
        public ListPrivateKeysCommand() : base("listprivatekeys")
        {
        }

        protected override void ExecuteCommand(string[] args)
        {
            Console.WriteLine("--------------------");
            Console.WriteLine("Private Keys in the wallet :");
            foreach (var aliased in WalletService.GetPrivateKeys())
            {
                Console.WriteLine($"{aliased.Alias} : Address {aliased.Data.Address.Encoded}");
            }
            Console.WriteLine("--------------------");
        }
    }
}