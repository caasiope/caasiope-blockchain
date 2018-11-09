using System;
using Caasiope.JSON.API.Responses;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Node.Processors.Commands
{
    public class SendTransactionCommand : LiveCommand<SignedTransaction>
    {
        private readonly Action<SendTransactionResponse, ResultCode> sendResponse;

        public SendTransactionCommand(SignedTransaction data, Action<SendTransactionResponse, ResultCode> sendResponse = null) : base(data)
        {
            this.sendResponse = sendResponse;
        }

        protected override ResultCode GetResult(SignedTransaction signed)
        {
            var resultCode = ResultCode.Failed;
            try
            {
                resultCode = LiveService.TransactionManager.ReceiveTransaction(signed);
            }
            catch (Exception e)
            {
                Logger.Log("SendTransactionCommand", e);
            }
            finally
            {
                sendResponse.Call(new SendTransactionResponse(), resultCode);
            }

            return resultCode;
        }
    }
}
