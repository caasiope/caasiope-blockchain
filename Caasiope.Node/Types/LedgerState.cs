using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Node.Managers;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Types
{
    public class ImmutableLedgerState : LedgerState
    {
        public readonly SignedLedger LastLedger;

        public ImmutableLedgerState(SignedLedger lastLedger)
        {
            LastLedger = lastLedger;
        }

        public IEnumerable<Account> GetAccounts()
        {
            throw new NotImplementedException();
            // TODO Optimize
            // return accounts.Values.Where(account => account.Balances.Any(balance => balance.Amount != 0));
        }
    }

    public class MutableLedgerState : LedgerState
    {
        private readonly ImmutableLedgerState previous;

        public MutableLedgerState(ImmutableLedgerState previous)
        {
            this.previous = previous;
            // create next state
            // throw new NotImplementedException();
        }

        public SignedLedger SignedLedger { get; set; }

        public SignedLedgerState GetLedgerStateChange()
        {
            return new SignedLedgerState(SignedLedger, GetStateChange());
        }

        public void SaveBalance(Account account, AccountBalance amount)
        {
            balances[new AddressCurrency(account.Address, amount.Currency)] = new AccountBalanceFull(account.Address, amount);
        }

        // TODO use the account history list to dynamicly compute the changes ?
        // we keep the list of the changes
        private readonly Dictionary<AddressCurrency, Protocol.Types.AccountBalanceFull> balances = new Dictionary<AddressCurrency, Protocol.Types.AccountBalanceFull>();
        private readonly List<MultiSignature> multisigToInclude = new List<MultiSignature>();
        private readonly List<HashLock> hashLocksToInclude = new List<HashLock>();
        private readonly List<TimeLock> timeLocksToInclude = new List<TimeLock>();
        
        // TODO we need to store a wrapper that contains both the mutable account and the changes
        private readonly Dictionary<Address, MutableAccount> accounts = new Dictionary<Address, MutableAccount>();

        public LedgerStateChange GetStateChange()
        {
            // useless everything is already immutable
            var results = balances.Values.Select(balance => new Protocol.Types.AccountBalanceFull(balance.Account, balance.AccountBalance)).ToList();

            return new LedgerStateChange(results, multisigToInclude, hashLocksToInclude, timeLocksToInclude);
        }

        public virtual void RemoveTransaction(Dictionary<TransactionHash, SignedTransaction> pendingTransactions, TransactionHash hash)
        {
            // TODO find why it bugs
            //Debug.Assert(pendingTransactions.ContainsKey(hash));
            pendingTransactions.Remove(hash);
        }

        // bad naming
        public virtual bool TryAddAccount(MultiSignature account)
        {
            if (ApplyDeclaration(account))
            {
                multisigToInclude.Add(account);
                return true;
            }
            return false;
            // Debug.Assert(isNew); // This is false, because account can be added and charged before the declaration sent
        }

        // bad naming
        public virtual bool TryAddAccount(HashLock account)
        {
            if (ApplyDeclaration(account))
            {
                hashLocksToInclude.Add(account);
                return true;
            }
            return false;

        }

        // bad naming
        public virtual bool TryAddAccount(TimeLock account)
        {
            if (ApplyDeclaration(account))
            {
                timeLocksToInclude.Add(account);
                return true;
            }
            return false;
        }

        // bad naming
        public bool ApplyDeclaration<T>(T declaration) where T : TxAddressDeclaration
        {
            var address = declaration.Address;
            // look in the state if it is already declared
            throw new NotImplementedException();
            // get and update in memory
            // accountmanager should also manage declarations
            /*
            var extended = LiveService.AccountManager.GetOrCreateAccount(address, () => new ExtendedAccount());
            // TODO handle concurrency
            if (extended.Declaration == null)
            {
                extended.Declaration = declaration;
                return true;
            }
            */
            return false;
        }

        public void SetBalance(MutableAccount account, Currency currency, Amount amount)
        {
            SaveBalance(account, account.SetBalance(currency, amount));
        }

        public ImmutableLedgerState Finalize()
        {
            throw new NotImplementedException();
            return new ImmutableLedgerState(SignedLedger);
        }

        public MutableAccount GetOrCreateMutableAccount(Address address)
        {
            // if we already modified it
            if(accounts.TryGetValue(address, out var account))
            {
                return account;
            }

            // try to get it into the previous state
            if (previous.TryGetAccount(address, out var old))
            {
                // make a new copy
                account = new MutableAccount(old.Address, old.Balances);
            }
            else
            {
                throw new NotImplementedException();
                // TODO use a callback
                // try to get it from the account manager
                // var extended = LiveService.AccountManager.GetOrCreateAccount(address, () => new ExtendedAccount());
            }

            accounts.Add(address, account);
            return account;
        }
    }

    public class LedgerState
    {
        public bool TryGetAccount(Address address, out Account account)
        {
            throw new NotImplementedException();
        }
    }
}
