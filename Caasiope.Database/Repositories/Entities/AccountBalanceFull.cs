using System.Diagnostics;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories.Entities
{
    [DebuggerDisplay("Account = {Account.Encoded} Currency = {Caasiope.Protocol.Types.Currency.ToSymbol(AccountBalance.Currency)}  Amount = {AccountBalance.Amount.value}")]
    public class AccountBalanceFull
    {
        public readonly Address Account;
        public readonly AccountBalance AccountBalance;

        public AccountBalanceFull(Address account, AccountBalance accountBalance)
        {
            Account = account;
            AccountBalance = accountBalance;
        }
    }
}