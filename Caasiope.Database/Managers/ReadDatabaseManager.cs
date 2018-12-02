using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;

namespace Caasiope.Database.Managers
{
    public class ReadDatabaseManager
    {
        private readonly RepositoryManager repositoryManager;

        public ReadDatabaseManager(RepositoryManager repositoryManager)
        {
            this.repositoryManager = repositoryManager;
        }


        private List<Account> GetAccountsInternal()
        {
            var list = new Dictionary<string, Account>();

            foreach (var entity in repositoryManager.GetRepository<AccountRepository>().GetEnumerable())
            {
                var account = ReadAccount(entity.Raw);
                list.Add(entity.Address.Encoded, account);
            }

            return list.Values.ToList();
        }

        private Account ReadAccount(byte[] raw)
        {
            using (var stream = new ByteStream(raw))
            {
                return stream.ReadAccount();
            }
        }

        // This we load to the state on initialization

        public List<Account> GetAccounts()
        {
            var list = new Dictionary<Address, Account>();

            foreach (var account in GetAccountsInternal())
            {
                list.Add(account.Address, account);
            }

            return list.Values.ToList();
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

        public HashSet<Address> GetAddresses()
        {
            using (var entities = new BlockchainEntities())
            {
                var addresses = new HashSet<Address>(entities.accounts.Select(_ => _.address).ToList().Select(Address.FromRawBytes));
                return addresses;
            }
        }
    }
}
