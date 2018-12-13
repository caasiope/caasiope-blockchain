using System;
using Caasiope.Node;
using Caasiope.Protocol.Extensions;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class GetAccountCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument addressArgument;

        // getaccount qyl68tygnjx6qqwrsmynmejmc9wxlw7almv3397j
        public GetAccountCommand() : base("getaccount")
        {
            addressArgument = RegisterArgument(new CommandArgument("address"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            var address = WalletService.GetAddress(addressArgument.Value);

            LiveService.AddCommand(new Node.Processors.Commands.GetAccountCommand(address.Encoded, (account, code) =>
            {
                if (code == ResultCode.Success)
                {
                    Console.WriteLine("----------------------");
                    DescribeAccount(account);
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

        // TODO make global function
        private void DescribeAccount(Account account)
        {
            var type = account.Address.Type;
            Console.WriteLine($"Account Type : {Enum.GetName(type.GetType(), type)}");

            if (type == AddressType.ECDSA)
                return;

            if (account.Declaration == null)
            {
                Console.WriteLine("This account has not been declared yet !");
                return;
            }

            switch (type)
            {
                case AddressType.MultiSignatureECDSA:
                    var multi = account.GetDeclaration<MultiSignature>();
                    Console.WriteLine($"Signatures Required : {multi.Required}");
                    Console.WriteLine("Signers Addresses :");
                    foreach (var signer in multi.Signers)
                        Console.WriteLine(signer.Encoded);
                    break;
                case AddressType.VendingMachine:
                    var machine = account.GetDeclaration<VendingMachine>();
                    Console.WriteLine($"Owner : {machine.Owner.Encoded}");
                    Console.WriteLine($"Currency In : {Currency.ToSymbol(machine.CurrencyIn)}");
                    Console.WriteLine($"Currency Out : {Currency.ToSymbol(machine.CurrencyOut)}");
                    Console.WriteLine($"Rate : {Amount.ToWholeDecimal(machine.Rate)}");
                    break;
            }
        }
    }
}