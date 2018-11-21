using System.Data.Entity;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Explorer.Database.SQL.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Repositories
{
    public class TransactionSignatureRepository : Repository<TransactionSignature, transactionsignature, string>
    {
        protected override DbSet<transactionsignature> GetDbSet(BlockchainEntities entities)
        {
            return entities.transactionsignatures;
        }

        protected override string GetKey(TransactionSignature item)
        {
            return item.TransactionHash.ToBase64() + item.Signature.PublicKey.ToBase64();
        }

        protected override transactionsignature ToEntity(TransactionSignature item)
        {
            return new transactionsignature() {publickey = item.Signature.PublicKey.GetBytes(),  signature = item.Signature.SignatureByte.Bytes, transaction_hash = item.TransactionHash.Bytes}; // Hash?
        }

        protected override TransactionSignature ToItem(transactionsignature entity)
        {
            return new TransactionSignature(new Signature(new PublicKey(entity.publickey), new SignatureByte(entity.signature)), new TransactionHash(entity.transaction_hash));
        }
    }
}
