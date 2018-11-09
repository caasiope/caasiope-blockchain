using System;
using System.Collections.Generic;
using Caasiope.JSON.API.Requests;
using Caasiope.JSON.API.Responses;
using Caasiope.JSON.Helpers;
using Caasiope.Node.Processors.Commands;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.Common.Logs;
using Helios.JSON;
using Caasiope.NBitcoin;

namespace Caasiope.Node.Connections
{
    public interface IDispatcher<in TSession>
    {
        bool Dispatch(TSession session, MessageWrapper wrapper, Action<Response, ResultCode> sendResponse);
    }

    public class Dispatcher : IDispatcher<IConnectionSession>
    {
		[Injected] public ILiveService LiveService;
		[Injected] public ILedgerService LedgerService;
        [Injected] public IDatabaseService DatabaseService;

        private readonly ILogger logger;

        public Dispatcher(ILogger logger)
        {
            this.logger = logger;
        }

        public bool Dispatch(IConnectionSession session, MessageWrapper wrapper, Action<Response, ResultCode> sendResponse)
        {
            if (wrapper.Data is Request)
            {
                DispatchRequest((Request) wrapper.Data, sendResponse);
                return true;
            }

            if (wrapper.Data is Response)
            {
                var responseWrapper = (ResponseMessage) wrapper;
                DispatchResponse(session, (Response)responseWrapper.Data, (ResultCode)(responseWrapper).ResultCode);
                return true;

            }
            if (wrapper.Data is Notification)
            {
                DispatchNotification(session, (Notification) wrapper.Data);
                return true;
            }
            return false;
        }

        private void DispatchRequest(Request request, Action<Response, ResultCode> sendResponse)
        {
            if (request is GetAccountRequest)
            {
                var message = (GetAccountRequest)request;

                var command = new GetAccountCommand(message.Address, (acc, rc) => sendResponse(ResponseHelper.CreateGetAccountResponse(acc), rc));
                LiveService.AddCommand(command);
            }
            else if (request is GetTransactionsRequest)
            {
                var message = (GetTransactionsRequest) request;

                if(!RequestHelper.TryReadTransactionHashes(message, out var hashes))
                {
                    sendResponse.Call(ResponseHelper.CreateGetTransactionResponse(), ResultCode.InvalidInputParam);
                    return;
                }

                LiveService.AddCommand(new GetTransactionsCommand(hashes, sendResponse));
            }
            else if (request is SendTransactionRequest)
            {
                var message = (SendTransactionRequest)request;
                if (!RequestHelper.TryReadSignedTransaction(message, out var signed))
                {
                    sendResponse.Call(new SendTransactionResponse(), ResultCode.CannotReadSignedTransaction);
                    return;
                }
                    
                LiveService.AddCommand(new SendTransactionCommand(signed, sendResponse));
            }
            else if (request is GetSignedLedgerRequest)
            {
                var message = (GetSignedLedgerRequest)request;

                if (message.Height < 0)
                {
                    sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponseFromZip(null), ResultCode.InvalidInputParam);
                    return;
                }

                var height = LedgerService.LedgerManager.GetLedgerLight().Height;

                if (message.Height > height)
                {
                    sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponseFromZip(null), ResultCode.LedgerDoesnotExist);
                    return;
                }

                if (message.Height == height)
                {
                    var ledger = LedgerService.LedgerManager.GetSignedLedger();
                    var zipped = LedgerCompressionEngine.ZipSignedLedger(ledger);
                    sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponseFromZip(zipped), ResultCode.Success);
                    return;
                }

                var signed = DatabaseService.ReadDatabaseManager.GetRawLedger(message.Height);
                if (signed == null)
                {
                    sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponseFromZip(null), ResultCode.LedgerDoesnotExist);
                }

                sendResponse.Call(ResponseHelper.CreateGetSignedLedgerResponseFromZip(signed), ResultCode.Success);
            }
            else if (request is GetCurrentLedgerHeightRequest)
            {
                var height = LedgerService.LedgerManager.GetLedgerLight().Height;
                sendResponse.Call(ResponseHelper.CreateGetCurrentLedgerHeightResponse(height), ResultCode.Success);
            }

            else
            {
                sendResponse.Call(new Response(), ResultCode.UnknownMessage);
            }
        }

        private void DispatchResponse(IConnectionSession session, Response response, ResultCode resultCode)
        {
            if (response is GetTransactionsResponse)
            {
                if (resultCode != ResultCode.Success)
                {
                    logger.Log($"GetTransactionsResponse: ResultCode {resultCode}");
                    return;
                } 

                var message = (GetTransactionsResponse) response;

                if (ResponseHelper.TryReadSignedTransactions(message, out var transactions))
                {
                    foreach (var signed in transactions)
                    {
                        LiveService.AddCommand(new SendTransactionCommand(signed));
                    }
                }
                else
                    logger.Log("GetTransactionsResponse: Cannot read SignedTransactions");
            }

            if (response is GetSignedLedgerResponse)
            {
                if (resultCode != ResultCode.Success)
                {
                    logger.Log($"GetSignedLedgerResponse: ResultCode {resultCode}");
                    return;
                }

                var message = (GetSignedLedgerResponse)response;

                if (!ResponseHelper.TryReadSignedLedger(message, out var signed))
                {
                    logger.Log("GetSignedLedgerResponse: Cannot read SignedLedger");
                    return;
                }

                var nextHeight = LedgerService.LedgerManager.GetNextHeight();
                if (signed.Ledger.LedgerLight.Height != nextHeight)
                {
                    logger.Log($"GetSignedLedgerResponse : Received height is not Next. Next height {nextHeight}, Received {signed.Ledger.LedgerLight.Height}");
                    return;
                }

                var ledger = LedgerService.LedgerManager.GetSignedLedger();
                if (!signed.Ledger.LedgerLight.Lastledger.Equals(ledger.Hash))
                {
                    logger.Log($"GetSignedLedgerResponse : PreviousLedger hash does not match. Current {ledger.Hash.ToBase64()}, Received prev {signed.Ledger.LedgerLight.Lastledger.ToBase64()}");
                    return;
                }

                if (!LedgerService.LedgerManager.ValidateSignatures(signed))
                {
                    logger.Log("GetSignedLedgerResponse: Ledger signatures are not valid");
                    return;
                }

                LedgerService.SetNextLedger(signed, () => { LiveService.CatchupManager.TryProcessHeight(session); });
            }
        }

        protected virtual void DispatchNotification(IConnectionSession session, Notification notification)
        {
            if (notification is JSON.API.Notifications.SignedNewLedger)
            {
                var message = (JSON.API.Notifications.SignedNewLedger)notification;
                if (!RequestHelper.TryReadSignedNewLedger(message, out var signed))
                    return;
                logger.Log($"SignedNewLedger received {signed.Height} hash: {signed.Hash.ToBase64()} Prev hash: {signed.PreviousLedgerHash.ToBase64()}");

                session.Session.Peer.Height = signed.Height;

                if (signed.PreviousLedgerHash.Equals(Hash256.Zero)) // we have received initial ledger with height 0
                {
                    logger.Log("SignedNewLedgerNotification: Received initial ledger with height 0. Skip");
                    return;
                }

                LiveService.AddCommand(new SignedNewLedgerCommand(signed, session));

            }

            if (notification is JSON.API.Notifications.TransactionReceived)
            {
                var message = (JSON.API.Notifications.TransactionReceived)notification;

                if (!RequestHelper.TryReadSignedTransaction(message, out var signed))
                    return;

                LiveService.AddCommand(new SendTransactionCommand(signed));
            }
        }
    }
}