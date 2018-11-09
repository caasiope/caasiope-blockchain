using System.Data.Entity;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Database.SQL.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories
{
    public class LedgerSignatureRepository : Repository<LedgerSignature, ledgersignature, string>
    {
        protected override DbSet<ledgersignature> GetDbSet(BlockchainEntities entities)
        {
            return entities.ledgersignatures;
        }

        protected override string GetKey(LedgerSignature item)
        {
            return item.LedgerHeight + item.Signature.PublicKey.ToBase64();
        }

        protected override ledgersignature ToEntity(LedgerSignature item)
        {
            return new ledgersignature { ledger_height= item.LedgerHeight, validator_publickey = item.Signature.PublicKey.GetBytes(), validator_signature = item.Signature.SignatureByte.Bytes }; // Hash?
        }

        protected override LedgerSignature ToItem(ledgersignature entity)
        {
            return new LedgerSignature(new Signature(new PublicKey(entity.validator_publickey), new SignatureByte(entity.validator_signature)), entity.ledger_height);
        }
    }
}

