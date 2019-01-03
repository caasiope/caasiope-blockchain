using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caasiope.Node;
using Caasiope.Node.Managers;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Services;
using Helios.Common.Extensions;
using Helios.Common.Synchronization;
using ThreadedService = Caasiope.Node.Services.ThreadedService;

namespace Caasiope.Explorer.Services
{
    public interface IOrderBookService : IService
    {
        List<Order> GetOrderBook(string symbol);
        IEnumerable<string> GetSymbols();
    }

    public class OrderBookService: ThreadedService, IOrderBookService
    {
        [Injected] public IExplorerDatabaseService ExplorerDatabaseService;
        [Injected] public IExplorerDataTransformationService ExplorerDataTransformationService;

        private SignedLedger ledger;
        private readonly OrderBookManager orderbooks = new OrderBookManager();

        protected override void OnInitialize()
        {
            Injector.Inject(orderbooks);
        }

        protected override void OnStart()
        {
            ExplorerDatabaseService.StartedHandle.WaitOne();
            ExplorerDataTransformationService.StartedHandle.WaitOne();
            ExplorerDataTransformationService.WaitTransformationCompleted();

            LedgerService.LedgerManager.SubscribeOnNewLedger(OnNewLedger);

            var machines = ExplorerDatabaseService.ReadDatabaseManager.GetVendingMachines();

            var vendingMachines = new List<Account>();
            foreach (var machine in machines)
            {
                if (LiveService.AccountManager.TryGetAccount(machine.Address, out var account))
                    vendingMachines.Add(account.Account);
            }

            orderbooks.Initialize(vendingMachines);
        }

        protected override void OnStop()
        {
        }

        protected override void Run()
        {
            orderbooks.ProcessNewLedger(ledger);

            ledger = null;
        }

        public List<Order> GetOrderBook(string symbol)
        {
            if (orderbooks.TryGetOrderBook(symbol, out var results))
                return results;
            return new List<Order>();
        }

        public void OnNewLedger(SignedLedger signed)
        {
            // todo use queue
            ledger = signed;
            trigger.Set();
        }

        public IEnumerable<string> GetSymbols()
        {
            return orderbooks.GetSymbols();
        }
    }

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

    public enum OrderSide
    {
        Buy = 1,
        Sell = 2
    }

    public class Symbol
    {
        public readonly string BaseCurrency;
        public readonly string QuoteCurrency;

        public Symbol(string baseCurrency, string quoteCurrency)
        {
            BaseCurrency = baseCurrency;
            QuoteCurrency = quoteCurrency;
        }

        public static implicit operator string(Symbol d)
        {
            return d.ToString();
        }

        public override string ToString()
        {
            return $"{BaseCurrency}/{QuoteCurrency}";
        }
    }

    public class Order
    {
        public readonly OrderSide Side;
        public Amount Size;
        public readonly Amount Price;
        public readonly Address Address;

        public Order(OrderSide side, Amount size, Amount price, Address address)
        {
            Debug.Assert(size != 0, "Size cannot be 0");
            Side = side;
            Size = size;
            Price = price;
            Address = address;
        }
    }
}