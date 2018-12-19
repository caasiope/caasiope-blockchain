using Caasiope.Database.Managers;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Protocol;
using Caasiope.Protocol.Compression;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.SqlTransactions
{
    public class SignedLedgerTransaction : Transaction<SignedLedgerState>
    {
        public SignedLedgerTransaction(SignedLedgerState data) : base(data) { }

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