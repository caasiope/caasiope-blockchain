using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Protocol.Types;

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