using System.Collections.Generic;
using System.Linq;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Chronicles;
using Caasiope.Node.Managers;
using AccountBalanceFull = Caasiope.Protocol.Types.AccountBalanceFull;

namespace Caasiope.Node.Sagas
{
    public abstract class UpdateStateSaga<T> : Saga<T>, IUpdateStateSaga
    {
        [Injected]
        public ILiveService LiveService;
        [Injected]
        public IDatabaseService DatabaseService;

        protected UpdateStateSaga()
        {
            Injector.Inject(this);
        }

        public virtual bool TryGetAccount(string encoded, out Account account)
        {
            return LiveService.AccountManager.TryGetAccount(encoded, out account);
        }

        public virtual void SaveBalance(Account account, AccountBalance amount)
        {
            balances[new AddressCurrency(account.Address, amount.Currency)] = new Protocol.Types.AccountBalanceFull(account.Address, amount);
        }

        private readonly Dictionary<AddressCurrency, Protocol.Types.AccountBalanceFull> balances = new Dictionary<AddressCurrency, Protocol.Types.AccountBalanceFull>();
        private readonly List<MultiSignature> multisigToInclude = new List<MultiSignature>();
        private readonly List<HashLock> hashLocksToInclude = new List<HashLock>();
        private readonly List<TimeLock> timeLocksToInclude = new List<TimeLock>();

        public LedgerStateChange GetStateChange()
        {
            var results = balances.Values.Select(balance => new Protocol.Types.AccountBalanceFull(balance.Account, balance.AccountBalance)).ToList();

            return new LedgerStateChange(results, multisigToInclude, hashLocksToInclude, timeLocksToInclude);
        }

        public virtual void RemoveTransaction(Dictionary<TransactionHash, SignedTransaction> pendingTransactions, TransactionHash hash)
        {
            // TODO find why it bugs
            //Debug.Assert(pendingTransactions.ContainsKey(hash));
            pendingTransactions.Remove(hash);
        }

        public virtual void AddAccount(Account account)
        {
            LiveService.AccountManager.AddAccount(account);
        }

        public virtual bool TryAddAccount(MultiSignature account)
        {
            if (LiveService.MultiSignatureManager.TryAddAccount(account))
            {
                multisigToInclude.Add(account);
                return true;
            }
            return false;
            // Debug.Assert(isNew); // This is false, because account can be added and charged before the declaration sent
        }

        public virtual bool TryAddAccount(HashLock account)
        {
            if (LiveService.HashLockManager.TryAddAccount(account))
            {
                hashLocksToInclude.Add(account);
                return true;
            }
            return false;

        }

        public virtual bool TryAddAccount(TimeLock account)
        {
            if (LiveService.TimeLockManager.TryAddAccount(account))
            {
                timeLocksToInclude.Add(account);
                return true;
            }
            return false;
        }

        public void SetBalance(Account account, Currency currency, Amount amount)
        {
            SaveBalance(account, account.SetBalance(currency, amount));
        }

        private class AddressCurrency
        {
            public readonly Address Address;
            public readonly Currency Currency;

            public AddressCurrency(Address address, Currency currency)
            {
                Address = address;
                Currency = currency;
            }

            public override bool Equals(object obj)
            {
                var currency = obj as AddressCurrency;
                return currency != null && Currency.Equals(currency.Currency) && Address.Equals(currency.Address);
            }

            protected bool Equals(AddressCurrency other)
            {
                return Equals(Address, other.Address) && Equals(Currency, other.Currency);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Address.GetHashCode() * 397) ^ Currency.GetHashCode();
                }
            }
        }
    }
}