using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    public static class AccountExtensions
    {
        // TODO it is a bit stupid since we have a dictionnary in the Account implementation
        public static Amount GetBalance(this Account account, Currency currency)
        {
            foreach (var balance in account.Balances)
            {
                if (balance.Currency.Equals(currency))
                    return balance.Amount;
            }

            return 0;
        }

        public static AccountBalance SetBalance(this MutableAccount account, Currency currency, Amount amount)
        {
            var balance = new AccountBalance(currency, amount);
            account.SetBalance(balance);

            return balance;
        }
    }
}