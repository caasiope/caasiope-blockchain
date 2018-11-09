using System.Collections.Generic;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Transformers
{
    // only transform after declaration was transformed
    internal class MultiSignatureAccountTransformerService : DataTransformerService<MultiSignatureEntity, MultiSignatureAccountRepository>
    {
        protected override IEnumerable<MultiSignatureEntity> Transform(DataTransformationContext context)
        {
            var multiSignatures = context.SignedLedgerState.State.MultiSignatures;
            var declarations = context.GetDeclarations();
            var list = new List<MultiSignatureEntity>();
            foreach (var multiSignature in multiSignatures)
            {
                list.Add(new MultiSignatureEntity(GetDeclarationId(declarations.AddressDeclarations, multiSignature.Address), multiSignature.Hash, new MultiSignatureAccount(multiSignature.Address, multiSignature.Required)));
            }
            return list;
        }

        private long GetDeclarationId(Dictionary<Address, TransactionDeclarationEntity> declarations, Address address)
        {
            return declarations[address].DeclarationId;
        }
    }
}