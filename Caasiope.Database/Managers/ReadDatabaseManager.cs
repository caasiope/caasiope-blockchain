using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Helios.Common.Extensions;
using System;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Database.SQL;
using Caasiope.Protocol;
using Caasiope.Protocol.Compression;
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

        // This we load to the state on initialization
        public List<MultiSignatureAddress> GetMultiSignatureAddresses()
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
        public List<HashLockAccount> GetHashLockAccounts()
        {
            return repositoryManager.GetRepository<HashLockRepository>().GetEnumerable().Select(_ => _.Account).ToList();
        }

        // This we load to the state on initialization
        public List<TimeLockAccount> GetTimeLockAccounts()
        {
            return repositoryManager.GetRepository<TimeLockRepository>().GetEnumerable().Select(_ => _.Account).ToList();
        }

        // This we load to the state on initialization
        public List<Account> GetAccounts()
        {
            var list = new Dictionary<string, Account>();

            foreach (var balance in repositoryManager.GetRepository<BalanceRepository>().GetEnumerable())
            {
                var address = balance.Account;
                list.GetOrCreate(address.Encoded, () => Account.FromAddress(address)).AddBalance(balance.AccountBalance);
            }

            return list.Values.ToList();
        }

        // This is used only in UnitTests to verify if transformation works correctly
        public SignedLedger GetLastLedger()
        {
            using (var entities = new BlockchainEntities())
            {
                var max = entities.ledgers.OrderByDescending(ledger => ledger.height).FirstOrDefault();

                if (max == null)
                {
                    return null;
                }

                return GetLedger(max.height);
            }
        }

        private SignedLedger GetLedger(long height)
        {
            var ledger = repositoryManager.GetRepository<LedgerRepository>().GetByKey(height);

            if (ledger == null)
                return null;

            var signatures = repositoryManager.GetRepository<LedgerSignatureRepository>().GetEnumerable().Where(s => s.LedgerHeight == height).Select(s => s.Signature).ToList();
            var block = repositoryManager.GetRepository<BlockRepository>().GetByKey(height);

            if (block == null)
                return null;

            var transactions = GetTransactions(height);

            var blockEnt = Block.CreateBlock(block.LedgerHeight, transactions, block.FeeTransactionIndex);
            Debug.Assert(blockEnt.Hash.Equals(new BlockHash(block.Hash)));

            var signed = new SignedLedger(new Ledger(ledger.Ledger, blockEnt, ledger.MerkleRootHash), signatures);
            Debug.Assert(signed.Hash.Equals(ledger.Hash));

            return signed;
        }

        private List<SignedTransaction> GetTransactions(long height)
        {
            var transactions = new List<SignedTransaction>();
            foreach (var simple in repositoryManager.GetRepository<TransactionRepository>().GetEnumerable().Where(transaction => transaction.LedgerHeight == height))
            {
                var transaction = GetTransaction(simple);
                var signatures = repositoryManager.GetRepository<TransactionSignatureRepository>().GetEnumerable().Where(s => s.TransactionHash.Equals(simple.TransactionHash)).Select(s => s.Signature).ToList();
                transactions.Add(new SignedTransaction(transaction, signatures));
            }
            return transactions;
        }

        private Transaction GetTransaction(SignedTransactionSimple simple)
        {
            var declarations = new List<TxDeclaration>();
            foreach (var declaration in repositoryManager.GetRepository<TransactionDeclarationRepository>().GetEnumerable(simple.TransactionHash))
            {
                var type = repositoryManager.GetRepository<DeclarationRepository>().GetByKey(declaration.DeclarationId).DeclarationType;

                if (type == DeclarationType.MultiSignature)
                    declarations.Add(GetMultiSignature(declaration.DeclarationId));

                else if (type == DeclarationType.TimeLock)
                    declarations.Add(GetTimeLock(declaration.DeclarationId));

                else if (type == DeclarationType.HashLock)
                    declarations.Add(GetHashLock(declaration.DeclarationId));

                else if (type == DeclarationType.Secret)
                    declarations.Add(GetSecret(declaration.DeclarationId));

                else throw new NotImplementedException();
            }

            var inputs = new List<TxInput>();
			var outputs = new List<TxOutput>();
			TxInput fees = null;

			foreach (var inputoutput in repositoryManager.GetRepository<TransactionInputOutputRepository>().GetEnumerable(simple.TransactionHash))
			{
				if (inputoutput.Index == 0)
				{
					Debug.Assert(inputoutput.TxInputOutput.IsInput);
					fees = new TxInput(inputoutput.TxInputOutput.Address, inputoutput.TxInputOutput.Currency, inputoutput.TxInputOutput.Amount);
					continue;
				}

				if (inputoutput.TxInputOutput.IsInput)
					inputs.Add(new TxInput(inputoutput.TxInputOutput.Address, inputoutput.TxInputOutput.Currency, inputoutput.TxInputOutput.Amount));
				else
					outputs.Add(new TxOutput(inputoutput.TxInputOutput.Address, inputoutput.TxInputOutput.Currency, inputoutput.TxInputOutput.Amount));
			}

			var message = repositoryManager.GetRepository<TransactionMessageRepository>().GetByKey(simple.TransactionHash);
			return new Transaction(declarations, inputs, outputs, message == null ? TransactionMessage.Empty : message.Message, simple.Expire, fees);
		}

        private MultiSignature GetMultiSignature(long id)
        {
            var multiSignature = repositoryManager.GetRepository<MultiSignatureAccountRepository>().GetByKey(id);

            var signers = new List<Address>();
            foreach (var signer in repositoryManager.GetRepository<MultiSignatureSignerRepository>().GetEnumerable())
            {
                if (signer.MultiSignature.Encoded != multiSignature.Account.Address.Encoded)
                    continue;
                signers.Add(signer.Signer);
            }

            return new MultiSignature(signers, multiSignature.Account.Required);
        }

        private TimeLock GetTimeLock(long id)
        {
            var timeLock = repositoryManager.GetRepository<TimeLockRepository>().GetByKey(id);
            return new TimeLock(timeLock.Account.Timestamp);
        }

        private SecretRevelation GetSecret(long id)
        {
            var multiSignature = repositoryManager.GetRepository<SecretRevelationRepository>().GetByKey(id);
            return multiSignature.SecretRevelation;
        }

        private HashLock GetHashLock(long id)
        {
            var hashLock = repositoryManager.GetRepository<HashLockRepository>().GetByKey(id);
            return new HashLock(hashLock.Account.SecretHash);
        }

        public Transaction GetTransaction(TransactionHash hash)
        {
            var simple = repositoryManager.GetRepository<TransactionRepository>().GetByKey(hash);
            return GetTransaction(simple);
        }

        public List<HistoricalTransaction> GetTransactionHistory(Address address, long? ledgerHeight)
        {
            var transactions = repositoryManager.GetRepository<TransactionRepository>();
            var inputs = repositoryManager.GetRepository<TransactionInputOutputRepository>().GetByAddress(address);

            var list = new Dictionary<TransactionHash, HistoricalTransaction>();
            foreach (var input in inputs)
            {
                var hash = input.TransactionHash;
                if (!list.ContainsKey(hash))
                {
                    var simple = transactions.GetByKey(hash);

                    if (ledgerHeight == null || simple.LedgerHeight > ledgerHeight)
                    {
                        list.Add(hash, new HistoricalTransaction(simple.LedgerHeight, GetTransaction(simple), simple.LedgerTimestamp));
                    }
                }
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
