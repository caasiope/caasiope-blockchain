﻿using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Protocol.Extensions;
using Caasiope.Protocol.MerkleTrees;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Types
{
    public class MutableLedgerState : LedgerState
    {
        public readonly SignedLedger SignedLedger;
        public long Height => SignedLedger.Ledger.LedgerLight.Height;

        public Action<MutableAccount> AccountCreated;

        public MutableLedgerState(LedgerState previous, SignedLedger ledger) : base(GetTree(previous).Clone())
        {
            SignedLedger = ledger;
        }

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
        private readonly Dictionary<AddressCurrency, AccountBalanceFull> balances = new Dictionary<AddressCurrency, AccountBalanceFull>();
        private readonly List<MultiSignature> multisigToInclude = new List<MultiSignature>();
        private readonly List<HashLock> hashLocksToInclude = new List<HashLock>();
        private readonly List<TimeLock> timeLocksToInclude = new List<TimeLock>();
        
        // TODO we need to store a wrapper that contains both the mutable account and the changes
        private readonly Dictionary<Address, MutableAccount> accounts = new Dictionary<Address, MutableAccount>();

        public LedgerStateChange GetStateChange()
        {
            var accountes = accounts.Values.Select(account => new AccountEntity(account.Address, account.CurrentLedger, account.Declaration != null)).ToList();
            return new LedgerStateChange(accountes, balances.Values.ToList(), multisigToInclude, hashLocksToInclude, timeLocksToInclude);
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
            SaveBalance(account, account.SetBalance(currency, amount));
        }

        public ImmutableLedgerState Finalize()
        {
            return new ImmutableLedgerState(SignedLedger, tree, HasherFactory.CreateHasher(SignedLedger.Ledger.LedgerLight.Version));
        }

        public MutableAccount GetOrCreateMutableAccount(Address address)
        {
            // if we already modified it
            if(accounts.TryGetValue(address, out var account))
            {
                return account;
            }

            tree.CreateOrUpdate(address.ToRawBytes(), old =>
            {
                // try to get it into the previous state
                if (old != null)
                {
                    // make a new copy
                    account = new MutableAccount(old, Height);
                }
                else
                {
                    account = new MutableAccount(address, Height);
                    AccountCreated(account);
                }

                return account;
            });

            accounts.Add(address, account);
            return account;
        }

        public override bool TryGetAccount(Address address, out Account account)
        {
            return tree.TryGetValue(address.ToRawBytes(), out account);
        }
    }
}