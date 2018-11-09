using System;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public abstract class AddInputOutputTransaction : InjectedConsoleCommand
    {
        private readonly CommandArgument addressArgument;
        private readonly CommandArgument currencyArgument;
        private readonly CommandArgument amountArgument;

        protected AddInputOutputTransaction(string name) : base(name)
        {
            addressArgument = RegisterArgument(new CommandArgument("address"));
            currencyArgument = RegisterArgument(new CommandArgument("currency"));
            amountArgument = RegisterArgument(new CommandArgument("amount"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var address = WalletService.GetAddress(addressArgument.Value);
            var currency = Currency.FromSymbol(currencyArgument.Value);
            var amount = Amount.FromWholeDecimal(Convert.ToDecimal(amountArgument.Value));

            var io = IsInput() ? (TxInputOutput) new TxInput(address, currency, amount) : new TxOutput(address, currency, amount);
            var text = io.IsInput ? "Input" : "Output";

            WalletService.PrepareTransactionManager.AddInputOutput(io);
            Console.WriteLine($"{text} Added!");
        }

        protected abstract bool IsInput();
    }
}