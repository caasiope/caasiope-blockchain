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
        public MutableAccount(Address address, long current) : base(address, current)
        {
        }

        // TODO make it static method with explicit name
        public MutableAccount(Account account, long current) : base(account.Address, current, account.Balances, account.Declaration)
        {
        }

        public MutableAccount SetBalance(AccountBalance balance)
        {
            balances[balance.Currency] = balance;
            return this;
        }

        public new MutableAccount SetBalances(IEnumerable<AccountBalance> balances)
        {
            base.SetBalances(balances);
            return this;
        }

        public MutableAccount SetDeclaration(TxAddressDeclaration declaration)
        {
            Declaration = declaration;
            return this;
        }

        public Account Finalize()
        {
            return this;
        }
    }

    // this account should be immutable
    [DebuggerDisplay("Address = {Address.Encoded}")]
    public class Account
    {
        public readonly Address Address;
        public IEnumerable<AccountBalance> Balances => balances.Values;
        public readonly long CurrentLedger;
        public TxAddressDeclaration Declaration { get; protected set; }

        protected readonly SortedDictionary<Currency, AccountBalance> balances = new SortedDictionary<Currency, AccountBalance>(new CurrencyComparer());

        protected Account(Address address, long current)
        {
            Address = address;
            CurrentLedger = current;
        }

        public Account(Address address, long current, IEnumerable<AccountBalance> balances, TxAddressDeclaration declaration = null) : this(address, current)
        {
            Declaration = declaration;
            SetBalances(balances);
        }

        protected void SetBalances(IEnumerable<AccountBalance> balances)
        {
            foreach (var balance in balances)
            {
                this.balances.Add(balance.Currency, balance);
            }
        }

        public Amount GetBalance(Currency currency)
        {
            if (balances.TryGetValue(currency, out var balance))
                return balance.Amount;

            return 0;
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
