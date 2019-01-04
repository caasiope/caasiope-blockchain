using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.Services;
using Caasiope.Node;
using Caasiope.Node.Managers;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;

namespace Caasiope.Explorer.Managers
{
    internal class OrderBookManager
    {
        // Thread safe

        [Injected] public ILiveService LiveService;

        private readonly Dictionary<string, Dictionary<Address, Order>> orders = new Dictionary<string, Dictionary<Address, Order>>();
        private readonly MonitorLocker locker = new MonitorLocker();

        private readonly HashSet<string> symbols = new HashSet<string>() { new Symbol("BTC", "LTC"), new Symbol("CAS", "BTC"), new Symbol("CAS", "LTC"), new Symbol("BTC", "ETH") };

        public void Initialize(List<Account> accounts)
        {
            foreach (var account in accounts)
            {
                var machine = (VendingMachine) account.Declaration;
                var symbol = GetSymbol(machine, out var side);
                var size = GetSize(account, machine);
                if (size == 0)
                    continue;

                var order = GetOrder(account, machine, side);
                orders.GetOrCreate(symbol).Add(account.Address, order);
            }
        }

        private Order GetOrder(Account account, VendingMachine machine, OrderSide side)
        {
            var size = GetSize(account, machine);
            var rate = machine.Rate;
            return new Order(side, size, rate, machine.Address);
        }

        private static Amount GetSize(Account account, VendingMachine machine)
        {
            var currencyOut = machine.CurrencyOut;
            var size = account.GetBalance(currencyOut);
            return size;
        }

        private string GetSymbol(VendingMachine machine, out OrderSide orderSide)
        {
            var inStr = Currency.ToSymbol(machine.CurrencyIn);
            var outStr = Currency.ToSymbol(machine.CurrencyOut);

            var symbol = new Symbol(inStr, outStr);
            if (symbols.Contains(symbol))
            {
                orderSide = OrderSide.Buy;
                return symbol;
            }

            var reversedSymbol = new Symbol(outStr, inStr);
            if (symbols.Contains(reversedSymbol))
            {
                orderSide = OrderSide.Sell;
                return reversedSymbol;
            }

            throw new NotImplementedException();
        }

        public bool TryGetOrderBook(string symbol, out List<Order> orderbook)
        {
            orderbook = null;
            using (locker.CreateLock())
            {
                if (orders.TryGetValue(symbol, out var dict))
                {
                    orderbook = dict.Values.ToList();
                    return true;
                }
            }

            return false;
        }

        public void ProcessNewLedger(SignedLedger ledger)
        {
            var accountsToUpdate = new Dictionary<Address, ExtendedAccount>();
            foreach (var transaction in ledger.Ledger.Block.Transactions)
            {
                foreach (var input in transaction.Transaction.Inputs)
                {
                    if (LiveService.AccountManager.TryGetAccount(input.Address, out var account))
                        if (account.Account.Address.Type == AddressType.VendingMachine)
                        {
                            accountsToUpdate.Add(account.Account.Address, account);
                        }
                }

                foreach (var input in transaction.Transaction.Outputs)
                {
                    if (accountsToUpdate.ContainsKey(input.Address))
                        continue;

                    if (LiveService.AccountManager.TryGetAccount(input.Address, out var account))
                        if (account.Account.Address.Type == AddressType.VendingMachine)
                        {
                            accountsToUpdate.Add(account.Account.Address, account);
                        }
                }
            }

            using (locker.CreateLock())
            {
                foreach (var account in accountsToUpdate.Values)
                {
                    var machine = (VendingMachine) account.Account.Declaration;
                    var symbol = GetSymbol(machine, out var side);

                    var oldOrders = orders.GetOrCreate(symbol, () => new Dictionary<Address, Order>() {{account.Account.Address, GetOrder(account.Account, machine, side)}});

                    var size = GetSize(account.Account, machine);
                    if (size == 0)
                        oldOrders.Remove(account.Account.Address);
                    else if (oldOrders.TryGetValue(account.Account.Address, out var oldOrder))
                        oldOrder.Size = size;
                }
            }
        }

        public IEnumerable<string> GetSymbols()
        {
            return symbols;
        }
    }
}