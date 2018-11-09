using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    public static class AccountExtensions
    {
        public static Amount GetBalance(this Account account, Currency currency)
        {
            foreach (var balance in account.Balances)
            {
                if (balance.Currency.Equals(currency))
                    return balance.Amount;
            }

            return 0;
        }

        public static AccountBalance SetBalance(this Account account, Currency currency, Amount amount)
        {
            var balance = new AccountBalance(currency, amount);
            account.RemoveBalance(currency);
            account.AddBalance(balance);

            return balance;
        }
    }
}