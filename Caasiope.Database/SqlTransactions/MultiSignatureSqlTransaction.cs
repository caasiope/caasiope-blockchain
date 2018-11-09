using Caasiope.Database.Managers;
using Caasiope.Database.SQL;
using Caasiope.Protocol.Types;
using Caasiope.Protocol;

namespace Caasiope.Database.SqlTransactions
{
    public class MultiSignatureSqlTransaction : SqlTransaction<MultiSignatureAddress>
    {
        public MultiSignatureSqlTransaction(MultiSignatureAddress data) : base(data)
        {
        }
        protected override void Populate(RepositoryManager repositories, BlockchainEntities entities)
        {
            /*
            var multis = repositories.GetRepository<MultiSignatureAccountRepository>();
            multis.CreateOrUpdate(entities, new MultiSignatureAccount(Data.Address, Data.Required));

            foreach (var signer in Data.Signers)
            {
                var signers = repositories.GetRepository<MultiSignatureSignerRepository>();
                signers.CreateOrUpdate(entities, new MultiSignatureSigner(Data.Address, signer));
            }
            */
        }
    }
}