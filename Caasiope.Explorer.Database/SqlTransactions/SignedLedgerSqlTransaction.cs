using Caasiope.Explorer.Database.Managers;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Protocol;
using Caasiope.Protocol.Compression;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.SqlTransactions
{
    public class SignedLedgerSqlTransaction : SqlTransaction<SignedLedgerState>
    {
        public SignedLedgerSqlTransaction(SignedLedgerState data) : base(data) { }

        protected override void Populate(RepositoryManager repositories, BlockchainEntities entities)
        {
            // Here we save only binarized ledger 
            var bytes = GetSignedLedgerBytes(Data.Ledger);
            var light = Data.Ledger.Ledger.LedgerLight;
            var ledgerRepository = repositories.GetRepository<LedgerRepository>();
            ledgerRepository.CreateOrUpdate(entities, new LedgerEntity(Data.Ledger.Hash, light, Data.Ledger.Ledger.MerkleHash, bytes));

            // TODO Save this after
            SaveState(repositories, entities, light.Height);
        }

        private void SaveState(RepositoryManager repositories, BlockchainEntities entities, long height)
        {
            var stateChangeRepository = repositories.GetRepository<LedgerStateChangeRepository>();
            stateChangeRepository.CreateOrUpdate(entities, new LedgerStateChangeSimple(height, GetLedgerStateChangeBytes(Data.State)));
        }

        private byte[] GetLedgerStateChangeBytes(LedgerStateChange change)
        {
            using (var stream = new ByteStream())
            {
                stream.Write(change);
                return stream.GetBytes();
            }
        }

        private byte[] GetSignedLedgerBytes(SignedLedger signedLedger)
        {
            using (var stream = new ByteStream())
            {
                stream.Write(signedLedger);
                return Zipper.Zip(stream.GetBytes());
            }
        }
    }
}
