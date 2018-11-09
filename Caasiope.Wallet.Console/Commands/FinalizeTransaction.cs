using System;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class FinalizeTransaction : InjectedConsoleCommand
    {
        public FinalizeTransaction() : base("finalizetransaction") { }

        protected override void ExecuteCommand(string[] args)
        {
            WalletService.PrepareTransactionManager.FinalizeTransaction();
            Console.WriteLine("Transaction Initialized !");
        }
    }
}