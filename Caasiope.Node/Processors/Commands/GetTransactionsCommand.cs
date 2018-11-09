using System;
using System.Collections.Generic;
using Caasiope.JSON.Helpers;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Helios.JSON;

namespace Caasiope.Node.Processors.Commands
{
    public class GetTransactionsCommand : LiveCommand<List<TransactionHash>>
    {
        private readonly Action<Response, ResultCode> sendResponse;
        public GetTransactionsCommand(List<TransactionHash> data, Action<Response, ResultCode> sendResponse) : base(data)
        {
            this.sendResponse = sendResponse;
        }

        protected override ResultCode GetResult(List<TransactionHash> hashes)
        {
            var transactions = new List<SignedTransaction>();

            foreach (var hash in hashes)
            {
                SignedTransaction transaction;
                if(LiveService.TransactionManager.TryGetTransaction(hash, out transaction))
                    transactions.Add(transaction);
            }


            sendResponse.Call(ResponseHelper.CreateGetTransactionResponse(transactions), ResultCode.Success);
            return ResultCode.Success;
        }
    }
}