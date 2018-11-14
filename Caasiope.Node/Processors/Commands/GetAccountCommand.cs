using System;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Node.Processors.Commands
{
    // the name and logic of this command are not satisfying
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
                resultCode = LedgerService.LedgerManager.LedgerState.TryGetAccount(new Address(address), out account) ? ResultCode.Success : ResultCode.UnknownAccount;
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