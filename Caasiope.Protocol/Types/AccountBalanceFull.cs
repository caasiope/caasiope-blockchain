using System.Diagnostics;

namespace Caasiope.Protocol.Types
{
    // TODO maybe wrong namespace
    [DebuggerDisplay("Account = {Account.Encoded} Currency = {Currency.ToSymbol(AccountBalance.Currency)}  Amount = {AccountBalance.Amount.value}")]
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