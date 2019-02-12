using Caasiope.Protocol.Types;

namespace Caasiope.Protocol.Extensions
{
    public static class AccountExtensions
    {
        public static AccountBalance SetBalance(this MutableAccount account, Currency currency, Amount amount)
        {
            var balance = new AccountBalance(currency, amount);
            account.SetBalance(balance);

            return balance;
        }

        public static T GetDeclaration<T>(this Account account) where T : TxAddressDeclaration
        {
            return (T) account.Declaration;
        }
    }
}