using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Repositories.Entities
{
    public class BlockEntity
    {
        // TODO It must be ulong. Unsigned types are not supported by EF
        public readonly long LedgerHeight;
        public readonly byte[] Hash;
        // TODO It must be ushort. Unsigned types are not supported by EF
        public readonly short? FeeTransactionIndex;

        public BlockEntity(Block block) : this(block.LedgerHeight, block.Hash.Bytes, block.FeeTransactionIndex) {}

        public BlockEntity(long ledgerHeight, byte[] hash, short? index)
        {
            this.LedgerHeight = ledgerHeight;
            this.Hash = hash;
            this.FeeTransactionIndex = index;
        }
    }
}