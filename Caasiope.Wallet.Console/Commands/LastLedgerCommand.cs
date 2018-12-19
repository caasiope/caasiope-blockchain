using System;
using System.Linq;
using Caasiope.Protocol.Formats;
using Caasiope.Protocol.Types;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class LastLedgerCommand : InjectedConsoleCommand
    {
        public LastLedgerCommand() : base("lastledger")
        {
        }

        protected override void ExecuteCommand(string[] args)
        {
            var ledger  = LedgerService.LedgerManager.LastLedger;
            var height = ledger.GetHeight();
            var hash = ledger.Hash;
            var timestamp = TimeFormat.ToDateTime(ledger.GetTimestamp());
            // var version = ledger.GetVersion();
            var count = ledger.Ledger.Block.Transactions.Count();

            Console.WriteLine("------------------------------");
            Console.WriteLine($"LastLedger Height {height} Hash {hash.ToBase64()} Timestamp {timestamp} Transactions {count}"); // Version {version.VersionNumber} 
        }
    }
}