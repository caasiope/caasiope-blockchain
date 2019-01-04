using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.JSON.API;
using Caasiope.Explorer.JSON.API.Requests;
using Caasiope.Explorer.Services;
using Caasiope.Explorer.Types;
using Caasiope.Node;
using Caasiope.Node.Connections;
using Caasiope.Node.Processors.Commands;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.Common.Logs;
using Helios.JSON;
using GetSignedLedgerRequest = Caasiope.Explorer.JSON.API.Requests.GetSignedLedgerRequest;
using HistoricalTransaction = Caasiope.Protocol.Types.HistoricalTransaction;
using ResponseHelper = Caasiope.Explorer.JSON.API.ResponseHelper;
using ResultCode = Caasiope.Node.ResultCode;

namespace Caasiope.Explorer
{
    public class Dispatcher : IDispatcher<ISession>
    {
        [Injected] public ILiveService LiveService;
        [Injected] public ILedgerService LedgerService;
        [Injected] public IDatabaseService DatabaseService;
        [Injected] public IExplorerDatabaseService ExplorerDatabaseService;
        [Injected] public IExplorerConnectionService ExplorerConnectionService;
        [Injected] public IOrderBookService OrderBookService;

        protected readonly ILogger Logger;

        public Dispatcher(ILogger logger)
        {
            Logger = logger;
        }

        public bool Dispatch(ISession session, MessageWrapper wrapper, Action<Response, ResultCode> sendResponse)
        {
            if (wrapper.Data is Request)
            {
                DispatchRequest(session, (Request)wrapper.Data, sendResponse);
                return true;
            }

            return false;
        }

        private void DispatchRequest(ISession session, Request request, Action<Response, ResultCode> sendResponse)
        {
            if (request is SendTransactionRequest)
            {
                var message = (SendTransactionRequest)request;
                if (!RequestHelper.TryReadSignedTransaction(message, out var signed))
                {
                    sendResponse.Call(ResponseHelper.CreateSendTransactionResponse(), ResultCode.CannotReadSignedTransaction);
                    return;
                }

                LiveService.AddCommand(new SendTransactionCommand(signed, (r, rc) =>
                {
                    if(rc == ResultCode.Success)
                        ExplorerConnectionService.SubscriptionManager.ListenTo(session, new TransactionTopic(signed.Hash));
                    sendResponse.Call(ResponseHelper.CreateSendTransactionResponse(signed.Hash), rc);
                }));
            }
            else if (request is GetSignedLedgerRequest)
            {
                var message = (GetSignedLedgerRequest)request;

                if (!message.Height.HasValue) // Get current ledger
                {
                    var ledger = LedgerService.LedgerManager.GetSignedLedger();
                    sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponse(ledger), ResultCode.Success);
                    return;
                }

                var height = message.Height.Value;

                if (height < 0)
                {
                    sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponse(), ResultCode.InvalidInputParam);
                    return;
                }

                var currentHeight = LedgerService.LedgerManager.GetSignedLedger().GetHeight();

                if (height > currentHeight)
                {
                    sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponse(), ResultCode.LedgerDoesnotExist);
                    return;
                }

                if (height == currentHeight)
                {
                    var ledger = LedgerService.LedgerManager.GetSignedLedger();
                    sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponse(ledger), ResultCode.Success);
                    return;
                }
                
                var signed = DatabaseService.ReadDatabaseManager.GetLedgerFromRaw(height);
                if (signed == null)
                {
                    sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponse(), ResultCode.LedgerDoesnotExist);
                    return;
                }

                sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponse(signed), ResultCode.Success);
            }
            else if (request is GetBalanceRequest)
            {
                var message = (GetBalanceRequest)request;

                var command = new GetAccountCommand(message.Address, (acc, rc) => sendResponse(ResponseHelper.CreateGetAccountResponse(acc), rc));
                LiveService.AddCommand(command);
            }
            else if (request is GetTransactionRequest)
            {
                var message = (GetTransactionRequest)request;
                byte[] hash;
                try
                {
                    hash = Convert.FromBase64String(message.Hash);
                }
                catch (Exception)
                {
                    sendResponse.Call(ResponseHelper.CreateGetTransactionResponse(), ResultCode.NotABase64);
                    return;
                }

                if (hash.Length != TransactionHash.SIZE)
                {
                    sendResponse.Call(ResponseHelper.CreateGetTransactionResponse(), ResultCode.NotAHash256);
                    return;
                }

                var transaction = ExplorerDatabaseService.ReadDatabaseManager.GetTransaction(new TransactionHash(hash));

                sendResponse.Call(ResponseHelper.CreateGetTransactionResponse(TransactionConverter.GetTransaction(transaction)), ResultCode.Success);
            }
            else if (request is GetTransactionHistoryRequest)
            {
                var message = (GetTransactionHistoryRequest)request;
                Address address;
                try
                {
                    address = new Address(message.Address);
                }
                catch (Exception)
                {
                    sendResponse.Call(ResponseHelper.CreateGetTransactionHistoryResponse(), ResultCode.ReadAddressFailure);
                    return;
                }

                if (message.Count <= 0)
                {
                    sendResponse.Call(ResponseHelper.CreateGetTransactionHistoryResponse(), ResultCode.InvalidInputParam);
                    return;
                }

                if (message.Page <= 0)
                {
                    sendResponse.Call(ResponseHelper.CreateGetTransactionHistoryResponse(), ResultCode.InvalidInputParam);
                    return;
                }

                // TODO this is a temporary hack
                {
                    var raw = ExplorerDatabaseService.ReadDatabaseManager.GetTransactionHistory(address, message.Height).OrderByDescending(_ => _.LedgerHeight).ToList();
                    var total = raw.Count;

                    raw = raw.Skip(message.Count * (message.Page - 1)).Take(message.Count).ToList();

                    var results = new List<HistoricalTransaction>();

                    foreach (var transaction in raw)
                    {
                        var ledger = DatabaseService.ReadDatabaseManager.GetLedgerFromRaw(transaction.LedgerHeight);
                        results.Add(new HistoricalTransaction(transaction.LedgerHeight, transaction.Transaction, ledger.GetTimestamp()));
                    }

                    var transactions = results.Select(TransactionConverter.GetHistoricalTransaction).ToList();
                    sendResponse.Call(ResponseHelper.CreateGetTransactionHistoryResponse(transactions, total), ResultCode.Success);
                }
            }
            else if (request is GetLedgerRequest)
            {
                var message = (GetLedgerRequest) request;

                if (message.Height != null && message.Height >= 0)
                {
                    var currentHeight = LedgerService.LedgerManager.GetSignedLedger().GetHeight();

                    if (message.Height > currentHeight)
                    {
                        sendResponse.Call(ResponseHelper.CreateGetLedgerResponse(), ResultCode.LedgerDoesnotExist);
                        return;
                    }

                    if (message.Height == currentHeight)
                    {
                        sendResponse.Call(ResponseHelper.CreateGetLedgerResponse(LedgerConverter.GetLedger(LedgerService.LedgerManager.GetSignedLedger())), ResultCode.Success);
                        return;
                    }

                    var signed = DatabaseService.ReadDatabaseManager.GetLedgerFromRaw(message.Height.Value);
                    if (signed == null)
                    {
                        sendResponse.Call(ResponseHelper.CreateGetLedgerResponse(), ResultCode.LedgerDoesnotExist);
                        return;
                    }

                    var ledger = LedgerConverter.GetLedger(signed);
                    sendResponse.Call(ResponseHelper.CreateGetLedgerResponse(ledger), ResultCode.Success);
                    return;
                }

                if (!string.IsNullOrEmpty(message.Hash))
                {
                    try
                    {
                        var bytes = Convert.FromBase64String(message.Hash);
                        var ledger = DatabaseService.ReadDatabaseManager.GetLedgerByHash(new LedgerHash(bytes));
                        sendResponse.Call(ResponseHelper.CreateGetLedgerResponse(LedgerConverter.GetLedger(ledger)), ResultCode.Success);
                        return;
                    }
                    catch (Exception e)
                    {
                        sendResponse.Call(ResponseHelper.CreateGetLedgerResponse(), ResultCode.InvalidInputParam);
                        return;
                    }
                }
                sendResponse.Call(ResponseHelper.CreateGetLedgerResponse(), ResultCode.InvalidInputParam);
            }
            else if (request is GetOrderBookRequest)
            {
                var message = (GetOrderBookRequest)request;

                var orderbook = OrderBookService.GetOrderBook(message.Symbol);
                sendResponse(ResponseHelper.CreateGetOrderBookResponse(OrderConverter.GetOrders(orderbook), message.Symbol), ResultCode.Success);
            }
            else if (request is SubscribeRequest)
            {
                var message = (SubscribeRequest) request;

                if (TopicsConverter.TryGetTopic(message.Topic, OrderBookService.GetSymbols(), out var topic))
                {
                    ExplorerConnectionService.SubscriptionManager.ListenTo(session, topic);
                    sendResponse(ResponseHelper.CreateSubscribeResponse(), ResultCode.Success);
                    return;
                }

                sendResponse(ResponseHelper.CreateSubscribeResponse(), ResultCode.InvalidInputParam);
            }
            else if (request is GetLatestLedgersRequest)
            {
                var height = LedgerService.LedgerManager.GetSignedLedger().GetHeight();

                var ledgers = DatabaseService.ReadDatabaseManager.GetLedgersFromHeight(height - 10);

                var results = ledgers.Select(LedgerConverter.GetLedger).ToList();
                sendResponse.Call(ResponseHelper.CreateGetLatestLedgersResponse(results), ResultCode.Success);
            }

            else
            {
                sendResponse.Call(new Response(), ResultCode.UnknownMessage);
            }
        }
    }

    public class TopicsConverter
    {
        public static bool TryGetTopic(JSON.API.Internals.Topic topic, IEnumerable<string> symbols, out Topic topicResult)
        {
            try
            {
                if (topic is JSON.API.Internals.AddressTopic address)
                    topicResult = new AddressTopic(new Address(address.Address));
                if (topic is JSON.API.Internals.LedgerTopic)
                    topicResult = new LedgerTopic();
                if (topic is JSON.API.Internals.OrderBookTopic orderBook)
                {
                    if (!symbols.Contains(orderBook.Symbol))
                    {
                        topicResult = null;
                        return false;
                    }

                    topicResult = new OrderBookTopic(orderBook.Symbol);
                }
                if (topic is JSON.API.Internals.TransactionTopic transaction)
                    topicResult = new TransactionTopic(new TransactionHash(Convert.FromBase64String(transaction.Hash)));
                if (topic is JSON.API.Internals.FundsTopic)
                    topicResult = new FundsTopic();

                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                topicResult = null;
                return false;
            }

        }
    }

    public class OrderConverter
    {
        public static List<JSON.API.Internals.Order> GetOrders(List<Order> orders)
        {
            return orders.Select(_ => new JSON.API.Internals.Order(GetSide(_.Side), Amount.ToWholeDecimal(_.Size), _.Price, _.Address.Encoded)).ToList(); // TODO Price??
        }

        private static char GetSide(OrderSide side)
        {
            return side == OrderSide.Buy ? 'b' : 's';
        }
    }
}
