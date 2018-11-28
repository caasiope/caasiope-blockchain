using System.Collections.Generic;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Explorer.Database.Repositories.Entities;

namespace Caasiope.Explorer.Transformers
{
    internal class BlockTransformerService : DataTransformerService<BlockEntity, BlockRepository>
    {
        protected override IEnumerable<BlockEntity> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;
            return new []{ new BlockEntity(signedLedgerState.Ledger.Ledger.Block) };
        }
    }
}