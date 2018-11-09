using System.Data.Entity;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.Types;
using Caasiope.Protocol;

namespace Caasiope.Database.Repositories
{
    public class MultiSignatureSignerRepository : Repository<MultiSignatureSigner, multisignaturesigner, string>
    {
        protected override DbSet<multisignaturesigner> GetDbSet(BlockchainEntities entities)
        {
            return entities.multisignaturesigners;
        }

        protected override string GetKey(MultiSignatureSigner item)
        {
            return item.MultiSignature.Encoded + item.Signer.Encoded;
        }

        protected override multisignaturesigner ToEntity(MultiSignatureSigner item)
        {
            return new multisignaturesigner()
            {
                multisignature_account = item.MultiSignature.ToRawBytes(),
                signer = item.Signer.ToRawBytes(),
            };
        }

        protected override MultiSignatureSigner ToItem(multisignaturesigner entity)
        {
            return new MultiSignatureSigner(Address.FromRawBytes(entity.multisignature_account), Address.FromRawBytes(entity.signer));
        }
    }
}