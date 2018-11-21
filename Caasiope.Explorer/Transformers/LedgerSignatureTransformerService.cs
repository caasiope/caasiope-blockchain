using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Transformers
{
    internal class LedgerSignatureTransformerService : DataTransformerService<LedgerSignature, LedgerSignatureRepository>
    {
        protected override IEnumerable<LedgerSignature> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;
            var list = new List<LedgerSignature>();
            foreach (var signature in signedLedgerState.Ledger.Signatures)
            {
                list.Add(new LedgerSignature(signature, signedLedgerState.Ledger.Ledger.LedgerLight.Height));
            }
            return list;
        }
    }
}