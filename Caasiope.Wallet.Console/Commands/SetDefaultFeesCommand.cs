using System;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class SetDefaultFeesCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument currencyArgument;
        private readonly CommandArgument amountArgument;

        public SetDefaultFeesCommand() : base("setdefaultfees")
        {
            currencyArgument = RegisterArgument(new CommandArgument("currency"));
            amountArgument = RegisterArgument(new CommandArgument("amount"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var currency = Currency.FromSymbol(currencyArgument.Value);
            var amount = Amount.FromWholeDecimal(Convert.ToDecimal(amountArgument.Value));
            WalletService.SetDefaultFees(currency, amount);
            Console.WriteLine("Default Fees set !");
        }
    }
}