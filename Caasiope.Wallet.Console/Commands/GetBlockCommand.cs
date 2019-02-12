using System;
using System.Linq;
using Caasiope.Protocol.Formats;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class GetBlockCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument idArgument;

        // getblock id
        public GetBlockCommand() : base("getblock")
        {
            //idArgument = RegisterArgument(new CommandArgument("height or hash"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            // determine if id or hash
            // fetch block in database

            var block  = LedgerService.LedgerManager.LastLedger.Ledger.Block;
            var height = block.LedgerHeight;
            var hash = block.Hash;
            var count = block.Transactions.Count();
            var fees = block.FeeTransactionIndex.HasValue ? Amount.ToWholeDecimal(block.Transactions.ElementAt(block.FeeTransactionIndex.Value).Transaction.Outputs[0].Amount) : 0;

            Console.WriteLine("------------------------------");
            Console.WriteLine($"Block Height {height} Hash {hash.ToBase64()} Transactions {count} Fees {fees} CAS");

            foreach (var transaction in block.Transactions)
            {
                DisplayFormatter.DisplayTransaction(transaction);
            }
        }
    }

    // does not work if we dont have explorer

    public static class DisplayFormatter
    {
        public static void DisplayTransaction(SignedTransaction signed)
        {
            var transaction = signed.Transaction;
            var hash = signed.Hash;
            var expire = TimeFormat.ToDateTime(transaction.Expire);
            var message = TransactionMessageFormat.ToString(transaction.Message);

            Console.WriteLine("------------------------------");
            Console.WriteLine($"Transaction Hash {hash.ToBase64()} Expire {expire} Message {message}");

            Console.WriteLine("Inputs :");
            foreach (var input in transaction.Inputs)
            {
                DisplayTxInputOutput(input);
            }

            Console.WriteLine("Outputs :");
            foreach (var output in transaction.Outputs)
            {
                DisplayTxInputOutput(output);
            }

            if (transaction.Fees != null)
            {
                DisplayTxInputOutput(transaction.Fees);
            }
        }

        private static void DisplayTxInputOutput(TxInputOutput inout)
        {
            var address = inout.Address.Encoded;
            var amount = Amount.ToWholeDecimal(inout.Amount);
            var currency = Currency.ToSymbol(inout.Currency);
            Console.WriteLine($"Address {address} Amount {amount} Currency {currency}");
        }
    }
}