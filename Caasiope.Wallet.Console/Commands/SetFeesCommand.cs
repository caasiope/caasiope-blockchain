using System;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class SetFeesCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument addressArgument;
        private readonly CommandArgument currencyArgument;
        private readonly CommandArgument amountArgument;

        public SetFeesCommand() : base("setfees")
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
            WalletService.PrepareTransactionManager.SetFees(address, currency, amount);
            Console.WriteLine("Fees Set !");
        }
    }
}