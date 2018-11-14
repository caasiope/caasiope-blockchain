using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caasiope.NBitcoin;
using HashLib;

namespace Caasiope.Protocol.Types
{
    // TODO this should be a wrapper
    public class MutableAccount : Account
    {
        public MutableAccount(Address address) : base(address)
        {
        }

        public MutableAccount(Address address, IEnumerable<AccountBalance> balances) : base(address, balances)
        {
        }

        public void SetBalance(AccountBalance balance)
        {
            balances[balance.Currency] = balance;
        }
    }

    // TODO this should be the default account class
    public class ImmutableAccount : Account
    {
        protected ImmutableAccount(Address address, List<AccountBalance> balances) : base(address, balances) { }
    }

    // TODO this should be the default account class
    public class AccountHistory
    {
        public readonly ImmutableAccount Account;
        public readonly long CurrentLedger;
        public readonly long PreviousLedger;
        public readonly bool IsDeclared; // this should be able to mute

        public AccountHistory(ImmutableAccount account, long currentLedger, long previousLedger, bool isDeclared)
        {
            Account = account;
            CurrentLedger = currentLedger;
            PreviousLedger = previousLedger;
            IsDeclared = isDeclared;
        }
    }

    [DebuggerDisplay("Address = {Address.Encoded}")]
    public class Account
    {
        public readonly Address Address;
        public IEnumerable<AccountBalance> Balances => balances.Values;

        protected readonly Dictionary<Currency, AccountBalance> balances = new Dictionary<Currency, AccountBalance>();

        protected Account(Address address)
        {
            Address = address;
        }

        public Account(Address address, IEnumerable<AccountBalance> balances) : this(address)
        {
            foreach (var balance in balances)
            {
                this.balances.Add(balance.Currency, balance);
            }
        }

        // TODO remove
        public static bool operator ==(Account a, Account b)
        {
            return a.Address == b.Address;
        }

        // TODO remove
        public static bool operator !=(Account a, Account b)
        {
            return !(a == b);
        }

        // TODO remove
        public Account Clone()
        {
            return new Account(Address, Balances.Select(b => new AccountBalance((short)b.Currency, (long)b.Amount)).ToList());
        }

        // used only for merkle tree
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

        // TODO move
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
