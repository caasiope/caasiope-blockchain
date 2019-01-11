using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.Services;
using Caasiope.Node;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;

namespace Caasiope.Explorer.Managers
{
    internal class OrderBookManager
    {
        // Thread safe

        [Injected] public ILedgerService LedgerService;
        public Action<string, List<Order>> OrderBookUpdated;

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
            var rate = Amount.ToWholeDecimal(machine.Rate);

            if (side == OrderSide.Sell)
                rate = 1 / rate;

            return new Order(side, size, rate, machine.Address);
        }

        private static decimal GetSize(Account account, VendingMachine machine)
        {
            var currencyOut = machine.CurrencyOut;
            var size = account.GetBalance(currencyOut);
            return Amount.ToWholeDecimal(size);
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
            var accountsToUpdate = GetChangedAccounts(ledger);

            var changes = UpdateOrderbook(accountsToUpdate);

            foreach (var change in changes)
            {
                if (change.Value)
                {
                    TryGetOrderBook(change.Key, out var orderbook);
                    OrderBookUpdated(change.Key, orderbook);
                }
            }
        }

        private Dictionary<string, bool> UpdateOrderbook(Dictionary<Address, Account> accountsToUpdate)
        {
            var changes = new Dictionary<string, bool>();
            using (locker.CreateLock())
            {
                foreach (var account in accountsToUpdate.Values)
                {
                    var machine = (VendingMachine) account.Declaration;
                    var symbol = GetSymbol(machine, out var side);

                    var oldOrders = orders.GetOrCreate(symbol, () => new Dictionary<Address, Order>() {{account.Address, GetOrder(account, machine, side)}});

                    var size = GetSize(account, machine);
                    if (size == 0)
                    {
                        oldOrders.Remove(account.Address);
                        changes[symbol] = true;
                    }
                    else if (oldOrders.TryGetValue(account.Address, out var oldOrder))
                    {
                        if (oldOrder.Size == size)
                            continue;

                        changes[symbol] = true;
                        oldOrder.Size = size;
                    }
                    else
                    {
                        changes[symbol] = true;
                        oldOrders.Add(account.Address, GetOrder(account, machine, side));
                    }
                }
            }

            return changes;
        }

        private Dictionary<Address, Account> GetChangedAccounts(SignedLedger ledger)
        {
            var accountsToUpdate = new Dictionary<Address, Account>();
            foreach (var transaction in ledger.Ledger.Block.Transactions)
            {
                foreach (var input in transaction.Transaction.Inputs)
                {
                    if (LedgerService.LedgerManager.LedgerState.TryGetAccount(input.Address, out var account))
                        if (account.Address.Type == AddressType.VendingMachine)
                        {
                            accountsToUpdate.Add(account.Address, account);
                        }
                }

                foreach (var input in transaction.Transaction.Outputs)
                {
                    if (accountsToUpdate.ContainsKey(input.Address))
                        continue;

                    if (LedgerService.LedgerManager.LedgerState.TryGetAccount(input.Address, out var account))
                        if (account.Address.Type == AddressType.VendingMachine)
                        {
                            accountsToUpdate.Add(account.Address, account);
                        }
                }
            }

            return accountsToUpdate;
        }

        public IEnumerable<string> GetSymbols()
        {
            return symbols;
        }
    }
}