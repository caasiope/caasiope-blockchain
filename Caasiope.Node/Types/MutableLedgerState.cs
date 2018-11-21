using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Protocol.Extensions;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Node.Types
{
    public class MutableLedgerState : LedgerState
    {
        private readonly ImmutableLedgerState previous;
        public long Height => previous.Height + 1;

        public Action<MutableAccount> AccountCreated;

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

        // TODO use the account history list to dynamicly compute the changes ?
        // we keep the list of the changes
        private readonly List<MultiSignature> multisigToInclude = new List<MultiSignature>();
        private readonly List<HashLock> hashLocksToInclude = new List<HashLock>();
        private readonly List<TimeLock> timeLocksToInclude = new List<TimeLock>();
        
        // TODO we need to store a wrapper that contains both the mutable account and the changes
        private readonly Dictionary<Address, MutableAccount> accounts = new Dictionary<Address, MutableAccount>();

        public LedgerStateChange GetStateChange()
        {
            var accountes = accounts.Values.Select(_ => (Account)_).ToList();
            return new LedgerStateChange(accountes, multisigToInclude, hashLocksToInclude, timeLocksToInclude);
        }

        public virtual void RemoveTransaction(Dictionary<TransactionHash, SignedTransaction> pendingTransactions, TransactionHash hash)
        {
            // TODO find why it bugs
            //Debug.Assert(pendingTransactions.ContainsKey(hash));
            pendingTransactions.Remove(hash);
        }

        // bad naming
        public virtual bool DeclareAccount(MultiSignature account)
        {
            if (TrySetDeclaration(account))
            {
                multisigToInclude.Add(account);
                return true;
            }
            return false;
            // Debug.Assert(isNew); // This is false, because account can be added and charged before the declaration sent
        }

        // bad naming
        public virtual bool DeclareAccount(HashLock account)
        {
            if (TrySetDeclaration(account))
            {
                hashLocksToInclude.Add(account);
                return true;
            }
            return false;

        }

        // bad naming
        public virtual bool DeclareAccount(TimeLock account)
        {
            if (TrySetDeclaration(account))
            {
                timeLocksToInclude.Add(account);
                return true;
            }
            return false;
        }

        // bad naming
        public bool TrySetDeclaration<T>(T declaration) where T : TxAddressDeclaration
        {
            var address = declaration.Address;
            // look in the state if it is already declared

            var account = GetOrCreateMutableAccount(address);
            
            if(account.Declaration != null)
                return false;

            account.SetDeclaration(declaration);
            return true;
        }

        public void SetBalance(MutableAccount account, Currency currency, Amount amount)
        {
            account.SetBalance(currency, amount);
        }

        public ImmutableLedgerState Finalize()
        {
            var dictionary = new Dictionary<Address,Account>();

            foreach (var pair in accounts)
                dictionary.Add(pair.Key, pair.Value);

            foreach (var account in previous.GetAccounts())
                dictionary.GetOrCreate(account.Address, () => account);

            return new ImmutableLedgerState(SignedLedger, dictionary);
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
                account = new MutableAccount(old, Height);
            }
            else
            {
                account = new MutableAccount(address, Height);
                AccountCreated(account);
            }

            accounts.Add(address, account);
            return account;
        }

        public override bool TryGetAccount(Address address, out Account account)
        {
            if (accounts.TryGetValue(address, out var mutable))
            {
                account = mutable;
                return true;
            }

            return previous.TryGetAccount(address, out account);
        }
    }
}