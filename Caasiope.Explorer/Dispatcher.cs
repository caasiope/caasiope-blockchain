using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Explorer.JSON.API;
using Caasiope.Explorer.JSON.API.Requests;
using Caasiope.Explorer.Services;
using Caasiope.Node;
using Caasiope.Node.Connections;
using Caasiope.Node.Processors.Commands;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.Common.Logs;
using Helios.JSON;
using GetSignedLedgerRequest = Caasiope.Explorer.JSON.API.Requests.GetSignedLedgerRequest;
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
                        ExplorerConnectionService.NotificationManager.ListenTo(session, signed.Hash);
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
            else
            {
                sendResponse.Call(new Response(), ResultCode.UnknownMessage);
            }
        }
    }
}
