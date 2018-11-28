using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using Caasiope.Explorer.Database.Repositories;
using Caasiope.Explorer.Database.Repositories.Entities;
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

                        list.Add(hash, new HistoricalTransaction(simple.LedgerHeight, GetTransaction(simple)));
                    }
                }
            }
            return list.Values.ToList();
        }

        public Transaction GetTransaction(TransactionHash hash)
        {
            var simple = repositoryManager.GetRepository<TransactionRepository>().GetByKey(hash);
            return GetTransaction(simple);
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
    }
}
