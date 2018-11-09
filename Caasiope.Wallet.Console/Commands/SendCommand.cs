using System;
using System.Collections.Generic;
using Caasiope.Protocol.Types;
using Helios.Common;
using Caasiope.NBitcoin;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class SendCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument receiverArgument;
        private readonly CommandArgument currencyArgument;
        private readonly CommandArgument amountArgument;

        // send qyl68tygnjx6qqwrsmynmejmc9wxlw7almv3397j BTC 1.3
        public SendCommand() : base("send")
        {
            receiverArgument = RegisterArgument(new CommandArgument("receiver"));
            currencyArgument = RegisterArgument(new CommandArgument("currency"));
            amountArgument = RegisterArgument(new CommandArgument("amount"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var sender = WalletService.GetActiveKey().Data.Account.Address;
            var receiver = WalletService.GetAddress(receiverArgument.Value);
            var currency = Currency.FromSymbol(currencyArgument.Value);
            var amount = Amount.FromWholeDecimal(Convert.ToDecimal(amountArgument.Value));

            var input = new TxInput(sender, currency, amount);
            var output = new TxOutput(receiver, currency, amount);

            var declarations = new List<TxDeclaration> { };
            var inputs = new List<TxInput> { input };
            var outputs = new List<TxOutput> { output };

            var fees = WalletService.CreateFeesInput(sender);

            var transaction = new Transaction(declarations, inputs, outputs, TransactionMessage.Empty, DateTime.UtcNow.AddMinutes(1).ToUnixTimestamp(), fees);

            if(WalletService.SignAndSubmit(transaction))
                Console.WriteLine($"Successfully sent transaction");
        }
    }
}