using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caasiope.NBitcoin;
using HashLib;

namespace Caasiope.Protocol.Types
{
	[DebuggerDisplay("Address = {Address.Encoded}")]
    public class Account
    {
        public readonly Address Address;
        public IEnumerable<AccountBalance> Balances => balances.Values;

        private readonly Dictionary<Currency, AccountBalance> balances = new Dictionary<Currency, AccountBalance>();

        private Account(Address address)
        {
            Address = address;
        }

        private Account(Address address, List<AccountBalance> balances) : this(address)
        {
            foreach (var balance in balances)
            {
                this.balances.Add(balance.Currency, balance);
            }
        }

        public AccountHash GetHash()
        {
            var sorted = SortBalances(Balances);
            return GetHash(sorted);
        }

        private static SortedList<int, AccountBalance> SortBalances(IEnumerable<AccountBalance> balances)
        {
            var list = new SortedList<int, AccountBalance>();
            foreach (var balance in balances)
            {
                list.Add(balance.Currency.GetHashCode(), balance);
            }
            return list;
        }

        public static Account FromAddress(Address address)
        {
            return new Account(address);
        }

        public static bool operator ==(Account a, Account b)
        {
            return a.Address == b.Address;
        }

        public static bool operator !=(Account a, Account b)
        {
            return !(a == b);
        }

        public Account Clone()
        {
            return new Account(Address, Balances.Select(b => new AccountBalance((short)b.Currency, (long)b.Amount)).ToList());
        }

        private AccountHash GetHash(SortedList<int, AccountBalance> balances)
        {
            using (var stream = new ByteStream())
            {
                stream.Write(Address);
                stream.Write(balances.Values.ToList(), stream.Write);

                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                var hash = hasher.ComputeBytes(message).GetBytes();
                return new AccountHash(hash);
            }
        }

        public void AddBalance(AccountBalance balance)
        {
            balances.Add(balance.Currency, balance);
        }

        public void RemoveBalance(Currency currency)
        {
            balances.Remove(currency);
        }
    }

    [DebuggerDisplay("Currency = {Currency.ToSymbol(Currency)}  Amount = {Amount.value}")]
    public class AccountBalance
    {
        public readonly Currency Currency;
        public readonly Amount Amount;

        public AccountBalance(Currency currency, Amount amount)
        {
            Currency = currency;
            Amount = amount;
        }
    }

    public class AccountHash : Hash256 {
        public AccountHash(byte[] bytes) : base(bytes)
        {
        }
    }
}
