using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Transformers
{
    internal class MultiSignatureSignerTransformerService : DataTransformerService<MultiSignatureSigner, MultiSignatureSignerRepository>
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