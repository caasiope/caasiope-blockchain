using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Caasiope.Protocol.MerkleTrees;
using Caasiope.Protocol.Types;
using Caasiope.NBitcoin;

namespace Caasiope.Protocol
{
    public class ByteStream : IDisposable
    {
        private readonly MemoryStream stream;

        public ByteStream()
        {
            stream = new MemoryStream();
        }

        public ByteStream(byte[] bytes)
        {
            stream = new MemoryStream(bytes);
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        protected void WriteNullable<T>(T data, Action<T> write)
        {
            if (data == null)
                Write(false);
            else
            {
                Write(true);
                write(data);
            }
        }

        protected void Write(bool value)
        {
            stream.Write(BitConverter.GetBytes(value), 0, 1);
        }

        protected void Write(long longData)
        {
            stream.Write(BitConverter.GetBytes(longData), 0, 8);
        }

        protected void Write(TransactionMessage message)
        {
            Write(message.GetBytes());
        }

        public void Write(LedgerStateChange change)
        {
            Write(change.Balances, Write);
            Write(change.MultiSignatures, Write);
            Write(change.HashLocks, Write);
            Write(change.TimeLocks, Write);
        }

        private void Write(AccountBalanceFull accountBalance)
        {
            Write(accountBalance.Account);
            Write(accountBalance.AccountBalance);
        }

        public void Write<T>(List<T> items, Action<T> write, int bytes = 1)
        {
            Debug.Assert(bytes > 0 && bytes <= 4);
            var count = items.Count;
            Debug.Assert(count < Math.Pow(256, bytes));
            stream.Write(BitConverter.GetBytes(count), 0, bytes);
            foreach (var item in items)
            {
                write(item);
            }
        }

        public List<T> ReadList<T>(Func<T> read, int bytes = 1)
        {
            Debug.Assert(bytes > 0 && bytes <= 4);
            var buffer = new byte[4];
            stream.Read(buffer, 0, bytes);
            var count = BitConverter.ToInt32(buffer, 0);

            var list = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                list.Add(read());
            }

            return list;
        }

        public void Write(TxInputOutput input)
        {
            Write(input.Address.ToRawBytes());
            Write(input.Currency);
            Write(input.Amount);
        }

        public void Write(TxDeclaration declaration)
        {
            Write((byte) declaration.Type);
            switch (declaration.Type)
            {
                case DeclarationType.MultiSignature:
                    Write((MultiSignature) declaration);
                    break;
                case DeclarationType.HashLock:
                    Write((HashLock) declaration);
                    break;
                case DeclarationType.TimeLock:
                    Write((TimeLock) declaration);
                    break;
                case DeclarationType.Secret:
                    Write((SecretRevelation) declaration);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected void Write(Amount amount)
        {
            stream.Write(BitConverter.GetBytes(amount), 0, 8);
        }

        protected void Write(Currency currency)
        {
            stream.Write(BitConverter.GetBytes(currency), 0, 2);
        }

        public void Write(SignedTransaction signed)
        {
            Write(signed.Transaction);
            Write(signed.Signatures, Write);
        }

        public void Write(Transaction transaction)
        {
            Write(transaction.Expire);
            WriteNullable(transaction.Fees, Write);
            Write(transaction.Declarations.ToList(), Write);
            Write(transaction.Inputs.ToList(), Write);
            Write(transaction.Outputs.ToList(), Write);
            WriteNullable(transaction.Message, Write);
        }

        protected void Write(MultiSignature multi)
        {
            Write(multi.Signers.ToList(), Write);
            Write(multi.Required);
        }

        protected void Write(TimeLock locker)
        {
            Write(locker.Timestamp);
        }

        protected void Write(HashLock locker)
        {
            Write(locker.SecretHash);
        }

        private void Write(SecretHash secret)
        {
            Write((byte) secret.Type);
            Write(secret.Hash);
        }

        private void Write(SecretRevelation secret)
        {
            Write(secret.Secret.Bytes);
        }

        protected void Write(byte data)
        {
            stream.WriteByte(data);
        }

        protected void Write(Signature signature)
        {
            Write(signature.PublicKey);
            Write(signature.SignatureByte);
        }

        public void Write(Key key)
        {
            Write(key.GetBytes());
        }

        protected void Write(SignatureByte signature)
        {
            Write(signature.Bytes);
        }
        
        public void Write(SignedLedger signed)
        {
            Write(signed.Ledger);
            Write(signed.Signatures, Write);
        }

        public void Write(Hash256 hash)
        {
            stream.Write(hash.Bytes, 0, Hash256.SIZE);
        }

        protected void Write(short data)
        {
            stream.Write(BitConverter.GetBytes(data), 0, 2);
        }

        public void Write(Account account)
        {
            Write(account.Address);
            Write(account.Balances.ToList(), Write);
        }

        public void Write(AccountBalance accountBalance)
        {
            Write(accountBalance.Currency);
            Write(accountBalance.Amount);
        }

        public void Write(Address address)
        {
            Write(address.ToRawBytes());
        }

        protected void Write(byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        public void Write(Ledger ledger)
        {
            Write(ledger.LedgerLight);
            Write(ledger.Block);
            Write(ledger.MerkleHash);
        }

        public void Write(LedgerLight ledger)
        {
            Write(ledger.Height);
            Write(ledger.Timestamp);
            Write(ledger.Lastledger.Bytes);
            stream.WriteByte(ledger.Version.VersionNumber);
        }

        protected void Write(Block block)
        {
            Write(block.LedgerHeight);
            Write(block.Transactions.ToList(), Write, 2);
            WriteNullable(block.FeeTransactionIndex, data => Write(data.Value));
        }

        public void Write(SignedNewLedger signed)
        {
            Write(signed.Hash);
            Write(signed.Height);
            Write(signed.Timestamp);
            Write(signed.PreviousLedgerHash);
            Write(signed.Signatures.ToList(), Write);
        }

        public byte[] GetBytes()
        {
            return stream.ToArray();
        }

        public SignedTransaction ReadSignedTransaction()
        {
            var transaction = ReadTransaction();
            return new SignedTransaction(transaction, ReadSignatures());
        }

        protected Signature ReadSignature()
        {
           return new Signature(ReadPublicKey(), new SignatureByte(stream.ReadBytes(SignatureByte.SIZE)));
        }

        protected PublicKey ReadPublicKey()
        {
            return new PublicKey(stream.ReadBytes(PublicKey.SIZE));
        }

        protected T ReadNullable<T>(Func<T> readFunc)
        {
            return ReadBool() ? readFunc() : default(T);
        }

        protected bool ReadBool()
        {
            return BitConverter.ToBoolean(stream.ReadBytes(1), 0);
        }

        public SignedLedger ReadSignedLedger()
        {
            return new SignedLedger(ReadLedger(), ReadSignatures());
        }

        protected Ledger ReadLedger()
        {
            return new Ledger(ReadLedgerLight(), ReadBlock(), ReadMerkleHash());
        }

        private LedgerMerkleRootHash ReadMerkleHash()
        {
            return new LedgerMerkleRootHash(stream.ReadBytes(LedgerMerkleRootHash.SIZE));
        }

        protected LedgerLight ReadLedgerLight()
        {
            return new LedgerLight(ReadLong(), ReadLong(), ReadLedgerHash(), ReadProtocolVersion());
        }

        protected LedgerHash ReadLedgerHash()
        {
            return new LedgerHash(stream.ReadBytes(LedgerHash.SIZE));
        }

        protected BlockHash ReadBlockHash()
        {
            return new BlockHash(stream.ReadBytes(BlockHash.SIZE));
        }

        protected Block ReadBlock()
        {
            return Block.CreateBlock(ReadLong(), ReadList(ReadSignedTransaction, 2), ReadNullable<short?>(() => ReadShort()));
        }

        protected ProtocolVersion ReadProtocolVersion()
        {
            return new ProtocolVersion(ReadByte());
        }

        protected byte ReadByte()
        {
            return stream.ReadBytes(1).First();
        }

        protected TransactionMessage ReadTransactionMessage()
        {
            return new TransactionMessage(stream.ReadBytes(TransactionMessage.SIZE));
        }

        protected TxInput ReadTxInput()
        {
            return new TxInput(ReadAddress(), ReadCurrency(), ReadAmount());
        }

        protected TxOutput ReadTxOutput()
        {
            return new TxOutput(ReadAddress(), ReadCurrency(), ReadAmount());
        }

        protected Amount ReadAmount()
        {
            return Amount.FromRaw(ReadLong());
        }

        protected long ReadLong()
        {
            return BitConverter.ToInt64(stream.ReadBytes(8), 0);
        }

        protected short ReadShort()
        {
            return BitConverter.ToInt16(stream.ReadBytes(2), 0);
        }

        protected Currency ReadCurrency()
        {
            return ReadShort();
        }

        protected Address ReadAddress()
        {
            return Address.FromRawBytes(stream.ReadBytes(Address.RAW_SIZE));
        }

        public Account ReadAccount()
        {
            return new Account(ReadAddress(), ReadAccountBalances());
        }

        protected List<AccountBalance> ReadAccountBalances()
        {
            return ReadList(ReadAccountBalance);
        }

        protected AccountBalance ReadAccountBalance()
        {
            return new AccountBalance(ReadCurrency(), ReadAmount());
        }

        public TransactionHash ReadTransactionHash()
        {
            return new TransactionHash(stream.ReadBytes(TransactionHash.SIZE));
        }
        
	    protected List<Signature> ReadSignatures()
        {
            return ReadList(ReadSignature);
        }

        public SignedNewLedger ReadSignedNewLedger()
        {
            return new SignedNewLedger(ReadLedgerHash(), ReadLong(), ReadLong(), ReadLedgerHash(), ReadSignatures());
        }

        protected Transaction ReadTransaction()
        {
            var expire = ReadLong();
            var fees = ReadNullable(ReadTxInput);
            
            return new Transaction(ReadList(ReadTxDeclaration), ReadList(ReadTxInput), ReadList(ReadTxOutput), ReadNullable(ReadTransactionMessage), expire, fees);
        }

        protected TxDeclaration ReadTxDeclaration()
        {
            var type = (DeclarationType)ReadByte();

            switch (type)
            {
                case DeclarationType.MultiSignature:
                    return ReadMultiSignature();
                case DeclarationType.HashLock:
                    return ReadHashLock();
                case DeclarationType.TimeLock:
                    return ReadTimeLock();
                case DeclarationType.Secret:
                    return ReadSecretRevelation();
                    default:
                        throw new NotImplementedException();
            }
        }

        private TimeLock ReadTimeLock()
        {
            return new TimeLock(ReadLong());
        }

        private TxDeclaration ReadSecretRevelation()
        {
            return new SecretRevelation(new Secret(stream.ReadBytes(Secret.SIZE)));
        }

        private HashLock ReadHashLock()
        {
            return new HashLock(ReadSecretHash());
        }

        private SecretHash ReadSecretHash()
        {
            return new SecretHash((SecretHashType)ReadByte(), new Hash256(stream.ReadBytes(Hash256.SIZE)));
        }

        protected MultiSignature ReadMultiSignature()
        {
            return new MultiSignature(ReadList(ReadAddress), ReadShort());
        }

        public LedgerStateChange ReadLedgerStateChange()
        {
            return new LedgerStateChange(ReadList(ReadAccountBalanceFull), ReadList(ReadMultiSignature), ReadList(ReadHashLock), ReadList(ReadTimeLock));
        }

        private AccountBalanceFull ReadAccountBalanceFull()
        {
            return new AccountBalanceFull(ReadAddress(), ReadAccountBalance());
        }
    }
}
