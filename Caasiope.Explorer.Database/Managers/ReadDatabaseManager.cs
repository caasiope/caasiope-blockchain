using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Explorer.Database.Repositories.Entities;
using Caasiope.Explorer.Database.SQL;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Managers
{
    public class ReadDatabaseManager
    {
        private readonly RepositoryManager repositoryManager;

        public ReadDatabaseManager(RepositoryManager repositoryManager)
        {
            this.repositoryManager = repositoryManager;
        }

        // This we load to the state on initialization
        private List<MultiSignatureAddress> GetMultiSignatureAddresses()
        {
            var list = new Dictionary<string, MultiSignatureAddress>();

            foreach (var entity in repositoryManager.GetRepository<MultiSignatureAccountRepository>().GetEnumerable())
            {
                var address = entity.Account.Address;
                var account = new MultiSignatureAddress(address, new List<Address>(), entity.Account.Required);
                list.Add(address.Encoded, account);
            }

            foreach (var signer in repositoryManager.GetRepository<MultiSignatureSignerRepository>().GetEnumerable())
            {
                // TODO optimize
                var encoded = signer.MultiSignature.Encoded;
                Debug.Assert(list.ContainsKey(encoded));
                list[encoded].Signers.Add(signer.Signer);
            }

            return list.Values.ToList();
        }

        // This we load to the state on initialization
        public List<Account> GetAccounts()
        {
            var list = new Dictionary<Address, MutableAccount>();

            foreach (var account in repositoryManager.GetRepository<AccountRepository>().GetEnumerable())
            {
                list.Add(account.Address, new MutableAccount(account.Address, account.CurrentLedgerHeight));
            }

            foreach (var balance in repositoryManager.GetRepository<BalanceRepository>().GetEnumerable())
            {
                list[balance.Account].SetBalance(balance.AccountBalance);
            }

            foreach (var multi in GetMultiSignatureAddresses())
            {
                list[multi.Address].SetDeclaration(new MultiSignature(multi.Signers, multi.Required));
            }

            foreach (var hashlock in repositoryManager.GetRepository<HashLockRepository>().GetEnumerable())
            {
                list[hashlock.Account.Address].SetDeclaration(new HashLock(hashlock.Account.SecretHash)); // TODO optimize
            }

            foreach (var timelock in repositoryManager.GetRepository<TimeLockRepository>().GetEnumerable())
            {
                list[timelock.Account.Address].SetDeclaration(new TimeLock(timelock.Account.Timestamp));
            }

            return list.Values.Select(mutable => mutable.Finalize()).ToList(); // TODO ugly
        }

        public SignedLedger GetLastLedgerFromRaw()
        {
            using (var entities = new BlockchainEntities())
            {
                var max = entities.ledgers.OrderByDescending(ledger => ledger.height).FirstOrDefault();
                Debug.Assert(max == null || max.raw != null && max.raw.Length != 0);
                return LedgerCompressionEngine.ReadZippedLedger(max?.raw);
            }
        }

        public SignedLedger GetLedgerFromRaw(long height)
        {
            return LedgerCompressionEngine.ReadZippedLedger(GetRawLedger(height));
        }

        public byte[] GetRawLedger(long height)
        {
            using (var entities = new BlockchainEntities())
            {
                var ledger = entities.ledgers.FirstOrDefault(_ => _.height == height);
                return ledger?.raw;
            }
        }

        public List<SignedLedgerState> GetLedgersFromHeight(long startHeight)
        {
            using (var entities = new BlockchainEntities())
            {
                var rawLedgers = entities.ledgers.Where(_ => _.height >= startHeight).ToList();
                var readedLedgers = rawLedgers.Select(_ => LedgerCompressionEngine.ReadZippedLedger(_.raw));

                var rawStateChanges = entities.ledgerstatechanges.Where(_ => _.ledger_height >= startHeight).ToList();
                var readedStateChanges = rawStateChanges.Select(_ => ReadStateChange(_.ledger_height, _.raw));

                var results = from ledger in readedLedgers
                    join state in readedStateChanges
                    on ledger.Ledger.LedgerLight.Height equals state.Item1
                    select new SignedLedgerState(ledger, state.Item2);

                return results.ToList();
            }
        }

        // todo compress this
        private Tuple<long, LedgerStateChange> ReadStateChange(long height, byte[] data)
        {
            if (data == null)
                return null;

            using (var stream = new ByteStream(data))
            {
                return new Tuple<long, LedgerStateChange>(height, stream.ReadLedgerStateChange());
            }
        }

        public List<TableLedgerHeight> GetHeightTables()
        {
            return repositoryManager.GetRepository<TableLedgerHeightRepository>().GetEnumerable().ToList();
        }

        public List<TransactionDeclarationEntity> GetDeclarations(long height)
        {
            var list = new List<TransactionDeclarationEntity>();
            var transactions = repositoryManager.GetRepository<TransactionRepository>().GetByHeight(height);
            foreach (var transaction in transactions)
                list.AddRange(repositoryManager.GetRepository<TransactionDeclarationRepository>().GetEnumerable(transaction.TransactionHash));
            return list;
        }
    }
}
