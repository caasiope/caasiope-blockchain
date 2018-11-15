using System;
using System.Collections.Generic;
using Caasiope.Protocol.Types;
using Caasiope.NBitcoin;
using HashLib;
using Helios.Common.Logs;

namespace Caasiope.Protocol.MerkleTrees
{
    public class LedgerMerkleRoot
    {
        private readonly ILogger logger;
        private readonly MerkleNode<AccountHash> accounts;
        private readonly MerkleNode<TxDeclarationHash> declarations;

        public readonly LedgerMerkleRootHash Hash;

        public LedgerMerkleRoot(IEnumerable<Account> accounts, IEnumerable<TxDeclaration> declarations, ILogger logger, Hasher hasher) : this(SortAccounts(accounts, hasher), SortDeclarations(declarations), logger) { }

        private LedgerMerkleRoot(SortedList<AccountHash, Account> accounts, SortedList<TxDeclarationHash, TxDeclaration> declarations, ILogger logger)
        {
            // TODO logger here looks ugly
            this.logger = logger;
            this.accounts = MerkleNode<AccountHash>.GetRoot(accounts.Keys);
            this.declarations = MerkleNode<TxDeclarationHash>.GetRoot(declarations.Keys);

            Hash = GetHash();

#if DEBUG
            DumpTree(accounts, declarations);
#endif
        }

        private void DumpTree(SortedList<AccountHash, Account> accountsDict, SortedList<TxDeclarationHash, TxDeclaration> declarationsDict)
        {
            logger.LogDebug("_______LedgerMerkleRoot Start___________");
            logger.LogDebug("_______Accounts:________________________");

            foreach (var account in accountsDict)
            {
                logger.LogDebug($"Hash: {account.Key.ToBase64()} Address: {account.Value.Address.Encoded}");
            }

            logger.LogDebug("_______Declarations:____________________");
            foreach (var declaration in declarationsDict)
            {
                logger.LogDebug($"Hash: {declaration.Key.ToBase64()} Type: {declaration.Value.Type}");
            }

            logger.LogDebug($"__Accounts Hash     : {accounts.Hash.ToBase64()}");
            logger.LogDebug($"__Declarations Hash : {declarations.Hash.ToBase64()}");
            logger.LogDebug($"__Merkle Hash       : {Hash.ToBase64()}");
        }

        private static SortedList<AccountHash, Account> SortAccounts(IEnumerable<Account> accounts, Hasher hasher)
        {
            var list = new SortedList<AccountHash, Account>();
            foreach (var account in accounts)
            {
                list.Add(hasher.GetHash(account), account);
            }
            return list;
        }

        private static SortedList<TxDeclarationHash, TxDeclaration> SortDeclarations(IEnumerable<TxDeclaration> declarations)
        {
            var list = new SortedList<TxDeclarationHash, TxDeclaration>();
            foreach (var declaration in declarations)
            {
                if (declaration.Type == DeclarationType.MultiSignature)
                {
                    var multisig = (MultiSignature)declaration;
                    list.Add(multisig.Hash, multisig);
                }
                else if (declaration.Type == DeclarationType.TimeLock)
                {
                    var timeLock = (TimeLock)declaration;
                    list.Add(timeLock.Hash, timeLock);
                }
                else if (declaration.Type == DeclarationType.HashLock)
                {
                    var hashLock = (HashLock)declaration;
                    list.Add(hashLock.Hash, hashLock);
                }
                else if (declaration.Type == DeclarationType.Secret)
                {
                    var secret = (SecretRevelation)declaration;
                    list.Add(secret.Hash, secret);
                }
                else
                    throw new NotImplementedException();
            }
            return list;
        }

        private LedgerMerkleRootHash GetHash()
        {
            using (var stream = new ByteStream())
            {
                stream.Write(accounts.Hash);
                stream.Write(declarations.Hash);

                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                var hash = hasher.ComputeBytes(message).GetBytes();
                return new LedgerMerkleRootHash(hash);
            }
        }
    }

    public class LedgerMerkleRootHash : Hash256
    {
        public LedgerMerkleRootHash(byte[] hash) : base(hash) { }
    }
}
