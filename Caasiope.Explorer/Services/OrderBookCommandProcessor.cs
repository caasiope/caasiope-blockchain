using System;
using System.Collections.Generic;
using Caasiope.Node;
using Caasiope.Node.Processors;
using Caasiope.Protocol.Types;
using Helios.Common.Logs;

namespace Caasiope.Explorer.Services
{

    internal interface IOrderBookCommand : Helios.Common.Concepts.CQRS.ICommand
    {
        ILogger Logger { get; set; }
    }

    internal class UpdateOrderbookCommand : OrderBookCommand<SignedLedger>
    {
        public UpdateOrderbookCommand(SignedLedger data) : base(data) { }

        protected override void Process(SignedLedger input)
        {
            OrderBookService.OrderBookManager.ProcessNewLedger(input);
        }
    }

    internal class GetOrderbookCommand : OrderBookCommand<string>
    {
        private readonly Action<List<Order>> callback;

        public GetOrderbookCommand(string data, Action<List<Order>> callback) : base(data)
        {
            this.callback = callback;
        }

        protected override void Process(string input)
        {
            try
            {
                callback(OrderBookService.OrderBookManager.TryGetOrderBook(input, out var results) ? results : new List<Order>());
            }
            catch (Exception)
            {
                callback(new List<Order>());
                throw;
            }
        }
    }

    internal abstract class OrderBookCommand<T> : Helios.Common.Concepts.CQRS.Command<T>, IOrderBookCommand
    {
        [Injected] public IOrderBookService OrderBookService;
        public ILogger Logger { set; get; }

        protected OrderBookCommand(T data) : base(data) { }

        protected sealed override void DoWork(T input)
        {
            try
            {
                Process(input);
            }
            catch (Exception e)
            {
                Logger.Log("exception", e);
                throw;
            }
        }

        protected abstract void Process(T input);
    }

    internal class OrderBookCommandProcessor : CommandProcessor<IOrderBookCommand>
    {
        private readonly ILogger logger;

        public OrderBookCommandProcessor(ILogger logger)
        {
            this.logger = logger;
        }

        protected override void PrepareCommand(IOrderBookCommand command)
        {
            base.PrepareCommand(command);
            command.Logger = logger;
        }
    }
}