using System.Collections.Generic;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Transformers
{
    internal class MultiSignatureSignerTransformerService : ExplorerDataTransformerService<MultiSignatureSigner, MultiSignatureSignerRepository>
    {
        protected override IEnumerable<MultiSignatureSigner> Transform(DataTransformationContext context)
        {
            var multiSignatures = context.SignedLedgerState.State.MultiSignatures;
            var list = new List<MultiSignatureSigner>();
            foreach (var multiSignature in multiSignatures)
            {
                foreach (var signer in multiSignature.Signers)
                {
                    list.Add(new MultiSignatureSigner(multiSignature.Address, signer));
                }
            }
            return list;
        }
    }
}