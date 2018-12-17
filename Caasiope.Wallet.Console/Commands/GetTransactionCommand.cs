using System.Linq;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class GetTransactionCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument idArgument;

        // TODO add block height ? use a cache ?
        // gettransaction id
        public GetTransactionCommand() : base("gettransaction")
        {
            //idArgument = RegisterArgument(new CommandArgument("height or hash"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            // determine if id or hash
            // fetch transaction in database

            var signed = LedgerService.LedgerManager.LastLedger.Ledger.Block.Transactions.First();
            DisplayFormatter.DisplayTransaction(signed);
        }
    }
}