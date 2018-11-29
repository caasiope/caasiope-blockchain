using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Database.Repositories;
using Caasiope.Database.Repositories.Entities;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Transformers
{
    internal class TransactionDeclarationTransformerService : DataTransformerService<TransactionDeclarationEntity, TransactionDeclarationRepository>
    {
        private class ProcessedDeclarations
        {
            public readonly Dictionary<MultiSignature, TransactionDeclarationEntity> MultiSignatures = new Dictionary<MultiSignature, TransactionDeclarationEntity>();
            public readonly Dictionary<HashLock, TransactionDeclarationEntity> HashLocks = new Dictionary<HashLock, TransactionDeclarationEntity>();
            public readonly Dictionary<TimeLock, TransactionDeclarationEntity> TimeLocks = new Dictionary<TimeLock, TransactionDeclarationEntity>();
            public readonly Dictionary<VendingMachine, TransactionDeclarationEntity> VendingMachines = new Dictionary<VendingMachine, TransactionDeclarationEntity>();

            public void Add(TransactionDeclarationEntity declarationEntity, TxDeclaration declaration)
            {
                switch (declaration.Type)
                {
                    case DeclarationType.MultiSignature:
                        var multi = (MultiSignature)declaration;
                        MultiSignatures[multi] = declarationEntity;
                        break;
                    case DeclarationType.HashLock:
                        var hashlock = (HashLock)declaration;
                        HashLocks[hashlock] = declarationEntity;
                        break;
                    case DeclarationType.TimeLock:
                        var timelock = (TimeLock)declaration;
                        TimeLocks[timelock] = declarationEntity;
                        break;
                    case DeclarationType.VendingMachine:
                        var machine = (VendingMachine)declaration;
                        VendingMachines[machine] = declarationEntity;
                        break;
                }
            }
        }

        protected override IEnumerable<TransactionDeclarationEntity> Transform(DataTransformationContext context)
        {
            var signedLedgerState = context.SignedLedgerState;

            var list = new List<TransactionDeclarationEntity>();
            var processed = new ProcessedDeclarations();
            var declarationContext = new TransactionDeclarationContext();
            var transactions = signedLedgerState.Ledger.Ledger.Block.Transactions;
            foreach (var transaction in transactions)
            {
                var hash = transaction.Hash;
                var index = 0;
                foreach (var declaration in transaction.Transaction.Declarations)
                {
                    var entity = new TransactionDeclarationEntity(hash, index++, GetDeclarationId(declaration, processed));
                    list.Add(entity);
                    processed.Add(entity, declaration);
                    declarationContext.TryAdd(entity, declaration);
                }
            }

            context.SetDeclarations(declarationContext);

            return list;
        }

        // we make sure that this transformer is called before other transformers of subtypes
        private long GetDeclarationId(TxDeclaration declaration, ProcessedDeclarations processed)
        {
            var repositoryManager = DatabaseService.RepositoryManager;
            long? id = null;

            TransactionDeclarationEntity processedDeclaration;
            switch (declaration.Type)
            {
                case DeclarationType.MultiSignature:
                    var multi = (MultiSignature)declaration;

                    // case 1 transactions in this batch (ledger) has same declaration several times. We take id of the first occurence
                    if (processed.MultiSignatures.TryGetValue(multi, out processedDeclaration))
                    {
                        id = processedDeclaration.DeclarationId;
                        break;
                    }

                    // Case 2 the original declaration was submitted before this batch (ledger)
                    id = repositoryManager.GetRepository<MultiSignatureAccountRepository>().GetByAddress(multi.Address)?.DeclarationId;
                    break;
                case DeclarationType.HashLock:
                    var hashlock = (HashLock)declaration;

                    // case 1 transactions in this batch (ledger) has same declaration several times. We take id of the first occurence
                    if (processed.HashLocks.TryGetValue(hashlock, out processedDeclaration))
                    {
                        id = processedDeclaration.DeclarationId;
                        break;
                    }

                    // Case 2 the original declaration was submitted before this batch (ledger)
                    id = repositoryManager.GetRepository<HashLockRepository>().GetByAddress(hashlock.Address)?.DeclarationId;
                    break;
                case DeclarationType.TimeLock:
                    var timelock = (TimeLock)declaration;

                    // case 1 transactions in this batch (ledger) has same declaration several times. We take id of the first occurence
                    if (processed.TimeLocks.TryGetValue(timelock, out processedDeclaration))
                    {
                        id = processedDeclaration.DeclarationId;
                        break;
                    }

                    // Case 2 the original declaration was submitted before this batch (ledger)
                    id = repositoryManager.GetRepository<TimeLockRepository>().GetByAddress(timelock.Address)?.DeclarationId;
                    break;
                case DeclarationType.VendingMachine:
                    var machine = (VendingMachine)declaration;

                    // case 1 transactions in this batch (ledger) has same declaration several times. We take id of the first occurence
                    if (processed.VendingMachines.TryGetValue(machine, out processedDeclaration))
                    {
                        id = processedDeclaration.DeclarationId;
                        break;
                    }

                    // Case 2 the original declaration was submitted before this batch (ledger)
                    id = repositoryManager.GetRepository<VendingMachineRepository>().GetByAddress(machine.Address)?.DeclarationId;
                    break;
            }

            // 

            // checked if this delaration already exist
            // get id from txdxEntity

            return id ?? Repository.GetNextId();
        }
    }
}