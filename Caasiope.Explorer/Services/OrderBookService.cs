using System;
using System.Collections.Generic;
using System.Diagnostics;
using Caasiope.Explorer.Managers;
using Caasiope.Node;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Services;
using ThreadedService = Caasiope.Node.Services.ThreadedService;

namespace Caasiope.Explorer.Services
{
    public interface IOrderBookService : IService
    {
        List<Order> GetOrderBook(string symbol);
        IEnumerable<string> GetSymbols();
        void OnOrderBookUpdated(Action<string, List<Order>> callback);
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
            LedgerService.StartedHandle.WaitOne();

            LedgerService.LedgerManager.SubscribeOnNewLedger(OnNewLedger);

            var machines = ExplorerDatabaseService.ReadDatabaseManager.GetVendingMachines();

            var vendingMachines = new List<Account>();
            foreach (var machine in machines)
            {
                if (LedgerService.LedgerManager.LedgerState.TryGetAccount(machine.Address, out var account))
                    vendingMachines.Add(account);
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

        public void OnOrderBookUpdated(Action<string, List<Order>> callback)
        {
            orderbooks.OrderBookUpdated += callback;
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
        public decimal Size;
        public readonly decimal Price;
        public readonly Address Address;

        public Order(OrderSide side, decimal size, decimal price, Address address)
        {
            Debug.Assert(size != 0, "Size cannot be 0");
            Side = side;
            Size = size;
            Price = price;
            Address = address;
        }
    }
}