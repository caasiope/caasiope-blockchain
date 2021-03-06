﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.Services;
using Caasiope.Node;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Explorer.Managers
{
    public class OrderBook
    {
        private readonly Dictionary<Address, Order> buyOrders = new Dictionary<Address, Order>();
        private readonly Dictionary<Address, Order> sellOrders = new Dictionary<Address, Order>();

        public void Add(Address address, Order order)
        {
            if(order.Side == OrderSide.Buy)
                buyOrders.Add(address, order);
            else
                sellOrders.Add(address, order);
        }

        public List<Order> GetOrderBook(int count)
        {
            // todo optimize
            var result = buyOrders.OrderByDescending(_ => _.Value.Price).Take(count).Select(_ => _.Value);
            return result.Union(sellOrders.OrderBy(_ => _.Value.Price).Take(count).Select(_ => _.Value)).ToList();
        }

        private void Remove(Address address, OrderSide side)
        {
            if (side == OrderSide.Buy)
                buyOrders.Remove(address);
            else
                sellOrders.Remove(address);
        }

        public bool TryUpdateOrder(Account account, VendingMachine machine, OrderSide side)
        {
            var size = GetSize(account, machine);

            if (size == 0)
            {
                Remove(account.Address, side);
                return true;
            }

            if (side == OrderSide.Buy && buyOrders.TryGetValue(account.Address, out var oldOrder) 
                || side == OrderSide.Sell && sellOrders.TryGetValue(account.Address, out oldOrder))
            {
                if (oldOrder.Size == size)
                    return false;

                oldOrder.Size = size;
                return true;
            }
            else
            {
                var newOrder = GetOrder(account, machine, side);
                Add(account.Address, newOrder);
                return true;
            }
        }

        public static Order GetOrder(Account account, VendingMachine machine, OrderSide side)
        {
            var size = GetSize(account, machine);
            var rate = Amount.ToWholeDecimal(machine.Rate);

            return new Order(side, size, rate, machine.Address);
        }

        public static decimal GetSize(Account account, VendingMachine machine)
        {
            var currencyOut = machine.CurrencyOut;
            var size = account.GetBalance(currencyOut);
            return Amount.ToWholeDecimal(size);
        }
    }
    public class OrderBookManager
    {
        [Injected] public ILedgerService LedgerService;
        public Action<string, List<Order>> OrderBookUpdated;

        private readonly Dictionary<string, OrderBook> orderBooks = new Dictionary<string, OrderBook>();
        private readonly HashSet<string> symbols = new HashSet<string>();

        public void Initialize(List<Account> accounts, List<Symbol> symbols)
        {
            foreach (var symbol in symbols)
            {
                this.symbols.Add(symbol);
            }

            foreach (var account in accounts)
            {
                var machine = (VendingMachine) account.Declaration;
                var symbol = GetSymbol(machine, out var side);
                var size = OrderBook.GetSize(account, machine);
                if (size == 0)
                    continue;

                var order = OrderBook.GetOrder(account, machine, side);
                orderBooks.GetOrCreate(symbol).Add(account.Address, order);
            }
        }

        private string GetSymbol(VendingMachine machine, out OrderSide orderSide)
        {
            var inStr = Currency.ToSymbol(machine.CurrencyIn);
            var outStr = Currency.ToSymbol(machine.CurrencyOut);

            var symbol = new Symbol(inStr, outStr);
            if (symbols.Contains(symbol))
            {
                orderSide = OrderSide.Sell;
                return symbol;
            }

            var reversedSymbol = new Symbol(outStr, inStr);
            if (symbols.Contains(reversedSymbol))
            {
                orderSide = OrderSide.Buy;
                return reversedSymbol;
            }

            throw new NotImplementedException($"Symbol doesn't exist for currencies {inStr}, {outStr}");
        }

        public bool TryGetOrderBook(string symbol, out List<Order> orderbook)
        {
            orderbook = null;
            if (orderBooks.TryGetValue(symbol, out var orderBook))
            {
                orderbook = orderBook.GetOrderBook(10);
                return true;
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
            foreach (var account in accountsToUpdate.Values)
            {
                var machine = (VendingMachine) account.Declaration;
                var symbol = GetSymbol(machine, out var side);


                var orderBook = orderBooks.GetOrCreate(symbol, () => new OrderBook());

                if (orderBook.TryUpdateOrder(account, machine, side))
                {
                    changes[symbol] = true;
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
                    if (accountsToUpdate.ContainsKey(input.Address))
                        continue;

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