using System;
using System.Collections.Generic;
using Caasiope.Protocol.Extensions;
using Caasiope.Protocol.MerkleTrees;

namespace Caasiope.Protocol.Types
{
    public class ImmutableLedgerState : LedgerState
    {
        public readonly SignedLedger LastLedger;
        public long Height => LastLedger.Ledger.LedgerLight.Height;

        public ImmutableLedgerState(SignedLedger lastLedger, Trie<Account> accounts, IHasher<Account> hasher) : base(accounts)
        {
            LastLedger = lastLedger;
            tree.ComputeHash(hasher);
        }

        public IEnumerable<Account> GetAccounts()
        {
            // TODO Optimize
            return tree.GetEnumerable();
        }

        public override bool TryGetAccount(Address address, out Account account)
        {
            return tree.TryGetValue(address.ToRawBytes(), out account);
        }

        public LedgerMerkleRootHash GetHash()
        {
            return new LedgerMerkleRootHash(tree.GetHash().Bytes);
        }
    }

    // TODO move in Node project
    public abstract class LedgerState
    {
        protected readonly Trie<Account> tree;

        protected LedgerState(Trie<Account> tree)
        {
            this.tree = tree;
        }

        protected static Trie<Account> GetTree(LedgerState state)
        {
            return state.tree;
        }

        public abstract bool TryGetAccount(Address address, out Account account);

        public bool TryGetDeclaration<T>(Address address, out T declaration) where T : TxAddressDeclaration
        {
            // if the account does exists in the state
            if (!TryGetAccount(address, out var account))
            {
                declaration = null;
                return false;
            }

            // check the account has been declared
            declaration = account.GetDeclaration<T>();
            return declaration != null;
        }
    }
}
