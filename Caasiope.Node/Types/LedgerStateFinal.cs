using System;
using System.Collections.Generic;
using Caasiope.Protocol.MerkleTrees;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Types
{
    public class LedgerStateFinal : LedgerState
    {
        public LedgerStateFinal(Trie<Account> accounts) : base(accounts)
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
}