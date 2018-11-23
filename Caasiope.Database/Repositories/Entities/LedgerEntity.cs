using Caasiope.Protocol.MerkleTrees;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories.Entities
{
    public class LedgerEntity
    {
        public readonly LedgerHash Hash;
        public readonly LedgerMerkleRootHash MerkleRootHash;
        public readonly LedgerLight Ledger;
        public readonly byte[] Raw;

        public LedgerEntity(LedgerHash hash, LedgerLight ledger, LedgerMerkleRootHash merkleRootHash, byte[] raw)
        {
            Hash = hash;
            Ledger = ledger;
            MerkleRootHash = merkleRootHash;
            Raw = raw;
        }
    }
}