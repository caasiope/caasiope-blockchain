using System;
using System.Collections.Generic;
using Caasiope.NBitcoin;
using Caasiope.Protocol.Extensions;
using Caasiope.Protocol.Types;
using Helios.Common;

namespace Caasiope.Wallet.CommandLineConsole.Commands
{
    public class ExchangeCommand : InjectedConsoleCommand
    {
        private readonly CommandArgument addressArgument;
        private readonly CommandArgument currencyArgument;
        private readonly CommandArgument amountArgument;

        // exchange qyl68tygnjx6qqwrsmynmejmc9wxlw7almv3397j BTC 1.3
        public ExchangeCommand() : base("exchange")
        {
            addressArgument = RegisterArgument(new CommandArgument("vending machine"));
            currencyArgument = RegisterArgument(new CommandArgument("currency"));
            amountArgument = RegisterArgument(new CommandArgument("amount"));
        }

        protected override void ExecuteCommand(string[] args)
        {
            // verify the vending machine
            var address = WalletService.GetAddress(addressArgument.Value);

            if (!LedgerService.LedgerManager.LedgerState.TryGetAccount(address, out var account))
            {
                Console.WriteLine($"The account {address} does not exist");
                return;
            }

            var type = account.Address.Type;
            if (type != AddressType.VendingMachine)
            {
                Console.WriteLine($"The account is not a vending machine : {Enum.GetName(type.GetType(), type)}");
                return;
            }

            var machine = account.GetDeclaration<VendingMachine>();
            if(machine == null)
            {
                Console.WriteLine("The account is not declared");
                return;
            }

            // compute the currency out
            var currency = Currency.FromSymbol(currencyArgument.Value);
            var amount = Amount.FromWholeDecimal(Convert.ToDecimal(amountArgument.Value));

            Amount amountIn, amountOut;

            var rate = machine.Rate;
            if (machine.CurrencyIn == currency)
            {
                amountIn = amount;
                amountOut = Amount.Divide(amount, rate);
            }
            else if (machine.CurrencyOut == currency)
            {
                amountOut = amount;
                amountIn = Amount.Multiply(amount, rate);
            }
            else
            {
                Console.WriteLine($"The machine can only receive {machine.CurrencyIn} and will only send {machine.CurrencyOut}");
                return;
            }

            var currencyIn = machine.CurrencyIn;
            var currencyOut = machine.CurrencyOut;

            // ask for confirmation
            Console.WriteLine($"Are you sure you cant to exchange {Amount.ToWholeDecimal(amountIn)} {Currency.ToSymbol(currencyIn)} against {Amount.ToWholeDecimal(amountOut)} {Currency.ToSymbol(currencyOut)} ? [y/n]");
            if (Console.ReadKey(true).Key != ConsoleKey.Y)
            {
                Console.WriteLine("Aborted...");
                return;
            }

            // create transaction
            var buyer = WalletService.GetActiveKey().Data.Address;
            var vendor = machine.Address;
            
            var declarations = new List<TxDeclaration> { };
            var inputs = new List<TxInput> { new TxInput(buyer, currencyIn, amountIn), new TxInput(vendor, currencyOut, amountOut) };
            var outputs = new List<TxOutput> { new TxOutput(vendor, currencyIn, amountIn), new TxOutput(buyer, currencyOut, amountOut) };

            var fees = WalletService.CreateFeesInput(buyer);

            var transaction = new Transaction(declarations, inputs, outputs, TransactionMessage.Empty, DateTime.UtcNow.AddMinutes(1).ToUnixTimestamp(), fees);

            if(WalletService.SignAndSubmit(transaction))
                Console.WriteLine($"Successfully sent transaction");
        }
    }
}