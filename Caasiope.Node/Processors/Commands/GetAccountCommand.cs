using System;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Node.Processors.Commands
{
    public class GetAccountCommand : LiveCommand<string>
    {
        private readonly Action<Account, ResultCode> onFinished;

        public GetAccountCommand(string data, Action<Account, ResultCode> onFinished) : base(data)
        {
            this.onFinished = onFinished;
        }

        protected override ResultCode GetResult(string address)
        {
            Account account = null;
            var resultCode = ResultCode.Failed;
            try
            {
                resultCode = LiveService.AccountManager.TryGetAccount(address, out account) ? ResultCode.Success : ResultCode.UnknownAccount;
            }
            catch (Exception e)
            {
                Logger.Log("GetAccountCommand", e);
            }
            finally
            {
                onFinished.Call(account, resultCode);
            }
            return resultCode;
        }
    }
}