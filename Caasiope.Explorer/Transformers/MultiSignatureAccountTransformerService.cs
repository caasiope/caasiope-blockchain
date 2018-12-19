using System.Collections.Generic;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Transformers
{
    // only transform after declaration was transformed
    internal class MultiSignatureAccountTransformerService : ExplorerDataTransformerService<MultiSignatureEntity, MultiSignatureAccountRepository>
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