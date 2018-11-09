using System;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class InitializeTransaction : InjectedConsoleCommand
    {
        public InitializeTransaction() : base("initializetransaction") { }

        protected override void ExecuteCommand(string[] args)
        {
            WalletService.PrepareTransactionManager.InitializeTransaction();
            Console.WriteLine("Transaction Initialized !");
        }
    }
}