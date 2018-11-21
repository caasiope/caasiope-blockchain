using System;
using System.Collections.Generic;
using Caasiope.Protocol.Extensions;
using Caasiope.Protocol.MerkleTrees;

namespace Caasiope.Protocol.Types
{
    public class ImmutableLedgerState : LedgerState
    {
        // public readonly long Height;
        // public readonly LedgerHash LastLedger;

        public ImmutableLedgerState(Trie<Account> accounts) : base(accounts)
        {
            if (!accounts.IsFinalized())
                throw new ArgumentException("The account trie should be finalized");
        }

        public IEnumerable<Account> GetAccounts()
        {
            // TODO Optimize
            return Tree.GetEnumerable();
        }

        public override bool TryGetAccount(Address address, out Account account)
        {
            return Tree.TryGetValue(address.ToRawBytes(), out account);
        }

        public LedgerMerkleRootHash GetHash()
        {
            return new LedgerMerkleRootHash(Tree.GetHash().Bytes);
        }
    }

    // TODO move in Node project
    public abstract class LedgerState
    {
        protected readonly Trie<Account> Tree;

        protected LedgerState(Trie<Account> tree)
        {
            Tree = tree;
        }

        protected static Trie<Account> GetTree(LedgerState state)
        {
            return state.Tree;
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
