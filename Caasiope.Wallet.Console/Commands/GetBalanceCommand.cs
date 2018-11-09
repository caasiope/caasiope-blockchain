using System;
using Caasiope.Node;
using Caasiope.Node.Processors.Commands;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class GetBalanceCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument addressArgument;

        // getbalance qyl68tygnjx6qqwrsmynmejmc9wxlw7almv3397j
        public GetBalanceCommand() : base("getbalance")
        {
            addressArgument = RegisterArgument(new CommandArgument("address"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var address = WalletService.GetAddress(addressArgument.Value);

            LiveService.AddCommand(new GetAccountCommand(address.Encoded, (account, code) =>
            {
                if (code == ResultCode.Success)
                {
                    Console.WriteLine("----------------------");
                    Console.WriteLine("Address : {0}", account.Address.Encoded);
                    foreach (var balance in account.Balances)
                    {
                        Console.WriteLine("{0} : {1}", Currency.ToSymbol(balance.Currency), Amount.ToWholeDecimal(balance.Amount));
                    }
                    Console.WriteLine("----------------------");
                }
                else
                {
                    Console.WriteLine("Get Balance Failed");
                }
            }));
        }
    }
}