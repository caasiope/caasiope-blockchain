using System;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class CreateAccountCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument aliasArgument;

        public CreateAccountCommand()
        {
            aliasArgument = RegisterArgument(new CommandArgument("alias"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var wallet = ConsoleHelper.GeneratePrivateKey();

            var alias = aliasArgument.Value;

            if (string.IsNullOrEmpty(alias.Trim()))
            {
                Console.WriteLine("Alias could not be empty string of whitespace");
                return;
            }

            if (WalletService.AliasManager.TryGetByAlias(alias, out var item))
            {
                Console.WriteLine($"Sorry, alias {alias} already exists, please try a different alias");
                return;
            }

            WalletService.ImportPrivateKey(alias, wallet);

            Console.WriteLine("-----------------------------------");
            wallet.Dump();
            Console.WriteLine("-----------------------------------");

            Console.WriteLine("Account created succsessfully!");
        }
    }
}