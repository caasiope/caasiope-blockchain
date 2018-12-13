using System;
using System.Collections.Generic;
using Caasiope.NBitcoin;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class CreateVendingMachineCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument aliasArgument;
        private readonly CommandArgument currencyInArgument;
        private readonly CommandArgument currencyOutArgument;
        private readonly CommandArgument rateArgument;
        private readonly CommandArgument amountArgument;

        public CreateVendingMachineCommand()
        {
            aliasArgument = RegisterArgument(new CommandArgument("alias"));
            currencyInArgument = RegisterArgument(new CommandArgument("currency in"));
            currencyOutArgument = RegisterArgument(new CommandArgument("currency out"));
            rateArgument = RegisterArgument(new CommandArgument("rate"));
            amountArgument = RegisterArgument(new CommandArgument("initial amount"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var owner = WalletService.GetActiveKey().Data.Address;
            var @in = Currency.FromSymbol(currencyInArgument.Value);
            var @out = Currency.FromSymbol(currencyOutArgument.Value);
            var rate = Amount.FromWholeDecimal(Decimal.Parse(rateArgument.Value));
            var machine = new VendingMachine(owner, @in, @out, rate);


            // TODO put this in a gobal function
            var alias = aliasArgument.Value;

            WalletService.ImportDeclaration(alias, machine);

            var amount = Amount.FromWholeDecimal(Convert.ToDecimal(amountArgument.Value));

            var sender = owner;
            var receiver = machine.Address;
            var currency = @out;

            // TODO put this in a gobal function
            var input = new TxInput(sender, currency, amount);
            var output = new TxOutput(receiver, currency, amount);

            var declarations = new List<TxDeclaration> { machine };
            var inputs = new List<TxInput> { input };
            var outputs = new List<TxOutput> { output };

            var fees = WalletService.CreateFeesInput(sender);

            var transaction = new Transaction(declarations, inputs, outputs, TransactionMessage.Empty, DateTime.UtcNow.AddMinutes(1).ToUnixTimestamp(), fees);

            if (WalletService.SignAndSubmit(transaction))
                Console.WriteLine($"Successfully sent transaction");
            // --
        }
    }
}