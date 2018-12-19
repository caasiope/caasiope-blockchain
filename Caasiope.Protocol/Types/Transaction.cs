using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Helios.Common.Extensions;
using HashLib;
using Caasiope.NBitcoin;

namespace Caasiope.Protocol.Types
{
    public class TransactionHash : Hash256 {
        public TransactionHash(byte[] bytes) : base(bytes){ }
    }

    public class TransactionSignature
    {
        public readonly Signature Signature;
        public readonly TransactionHash TransactionHash;

        public TransactionSignature(Signature signature, TransactionHash hash)
        {
            Signature = signature;
            TransactionHash = hash;
        }
    }

    public class Transaction
    {
        public readonly long Expire;
        public readonly TxInput Fees;
        public readonly List<TxDeclaration> Declarations;
        public readonly List<TxInput> Inputs;
        public readonly List<TxOutput> Outputs;
        public readonly TransactionMessage Message;


        public Transaction(List<TxDeclaration> declarations, List<TxInput> inputs, List<TxOutput> outputs, TransactionMessage message, long expire, TxInput fees = null)
        {
            Expire = expire;
            Fees = fees;
            Declarations = declarations;
            Inputs = inputs;
            Outputs = outputs;
            Message = message;
        }

        public TransactionHash GetHash() { return ComputeHash(); }
        
        public List<TxInput> GetInputs()
        {
            var list = Fees == null ? new List<TxInput>() : new List<TxInput>() { Fees };
            list.AddRange(Inputs);
            return list;
        }

        protected TransactionHash ComputeHash()
        {
            var message = ToBytes();
            var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
            return new TransactionHash(hasher.ComputeBytes(message).GetBytes());
        }

        public static IEnumerable<TxInputOutput> SortInputOutputs(IEnumerable<TxInputOutput> list)
        {
            return list.OrderBy(input => input.Address.Encoded + Currency.ToSymbol(input.Currency));
        }

        public byte[] ToBytes()
        {
            using (var stream = new ByteStream())
            {
                stream.Write(this);
                return stream.GetBytes();
            }
        }
    }

    public enum DeclarationType : byte
    {
        MultiSignature = 0x0,
        HashLock = 0x1,
        Secret = 0x2,
        TimeLock = 0x3,
        VendingMachine = 0x4,
    }

    public abstract class TxDeclaration : IEquatable<TxDeclaration>
    {
        public readonly DeclarationType Type;

        protected TxDeclaration(DeclarationType type)
        {
            Type = type;
        }

        public bool Equals(TxDeclaration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TxDeclaration) obj);
        }

        public override int GetHashCode()
        {
            return (int) Type;
        }
    }

    public abstract class TxAddressDeclaration : TxDeclaration, IEquatable<TxAddressDeclaration>
    {
        protected TxAddressDeclaration(DeclarationType type) : base(type) { }
        public Address Address { get; protected set; }

        public bool Equals(TxAddressDeclaration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Address, other.Address);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TxAddressDeclaration) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Address.GetHashCode();
            }
        }
    }

    public class TxDeclarationHash : Hash256
    {
        public TxDeclarationHash(byte[] bytes) : base(bytes) { }
    }

    public abstract class Transaction<THash>
    {
        public abstract THash GetHash();
    }

    public abstract class TxInputOutput
    {
        public readonly bool IsInput;
        public readonly Address Address;
        public readonly Currency Currency;
        public readonly Amount Amount;

        protected TxInputOutput(Address address, Currency currency, Amount amount, bool isInput)
        {
            Address = address;
            Currency = currency;
            Amount = amount;
            IsInput = isInput;
        }

        public override bool Equals(object obj)
        {
            var input = obj as TxInputOutput;
            if (input == null)
                return false;
            return IsInput == input.IsInput
                && Address == input.Address
                && Currency == input.Currency
                && Amount == input.Amount;
        }
    }

    public class TxInput : TxInputOutput
    {
        public TxInput(Address address, Currency currency, Amount amount) : base(address, currency, amount, true)
        {
        }
    }

    public class TxOutput : TxInputOutput
    {
        public TxOutput(Address address, Currency currency, Amount amount) : base(address, currency, amount, false)
        {
        }
    }

    public class TransactionMessage
    {
        public const int SIZE = 10;
        private readonly byte[] bytes = new byte[SIZE];

        public TransactionMessage(byte[] bytes)
        {
            Debug.Assert(bytes.Length == SIZE);
            this.bytes = bytes;
        }

        public byte[] GetBytes()
        {
            return bytes;
        }

        public override bool Equals(object obj)
        {
            return obj is TransactionMessage o && bytes.IsEqual(o.GetBytes());
        }

        public override int GetHashCode()
        {
            return Utils.GetHashCode(bytes);
        }

        public static readonly TransactionMessage Empty = null;
    }

    public static class TransactionMessageFormat
    {

        public static TransactionMessage FromString(string message)
        {
            return new TransactionMessage(Encoding.ASCII.GetBytes(message));
        }

        public static string ToString(TransactionMessage message)
        {
            return Encoding.ASCII.GetString(message.GetBytes());
        }

        public static TransactionMessage FromLong(long value)
        {
            return new TransactionMessage(BitConverter.GetBytes(value));
        }

        public static long ToLong(TransactionMessage message)
        {
            return BitConverter.ToInt64(message.GetBytes(), 0);
        }

        public static TransactionMessage FromDouble(double value)
        {
            return new TransactionMessage(BitConverter.GetBytes(value));
        }

        public static double ToDouble(TransactionMessage message)
        {
            return BitConverter.ToDouble(message.GetBytes(), 0);
        }
    }

    public static class TransactionMessageExtensions
    {
        public static bool IsEmpty(this TransactionMessage transaction)
        {
            return transaction == null;
        }
    }

    public class TransactionEqualityComparer : IComparer<SignedTransaction>
    {
        public int Compare(SignedTransaction x, SignedTransaction y)
        {
            if (x.Transaction.Expire == y.Transaction.Expire && x.Hash.Equals(y.Hash))
                return 0;
            if (x.Transaction.Expire > y.Transaction.Expire && x.Hash.Equals(y.Hash))
                return 1;
            else
                return -1;
        }
    }

    public class MultiSignature : TxAddressDeclaration
    {
        public readonly IEnumerable<Address> Signers;
        public readonly short Required;

        public readonly MultiSignatureHash Hash;

        public MultiSignature(IEnumerable<Address> signers, short required) : this(SortSigners(signers), required) { }

        private MultiSignature(SortedList<string, Address> signers, short required) : base(DeclarationType.MultiSignature)
        {
            Signers = signers.Values;
            Required = required;
            Hash = ComputeHash();
            Address = new Address(AddressType.MultiSignatureECDSA, Hash.Bytes.SubArray(0, Address.RAW_SIZE - 1));
        }

        private static SortedList<string, Address> SortSigners(IEnumerable<Address> signers)
        {
            var list = new SortedList<string, Address>();
            foreach (var transaction in signers)
            {
                list.Add(transaction.Encoded, transaction);
            }
            return list;
        }

        private MultiSignatureHash ComputeHash()
        {
            using (var stream = new ByteStream())
            {
                stream.Write(this);
                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                return new MultiSignatureHash(hasher.ComputeBytes(message).GetBytes());
            }
        }
    }

    public class MultiSignatureHash : TxDeclarationHash
    {
        public MultiSignatureHash(byte[] bytes) : base(bytes) { }
    }

    public class HashLock : TxAddressDeclaration
    {
        public readonly SecretHash SecretHash;
        public readonly HashLockHash Hash;

        public HashLock(SecretHash secret) : base(DeclarationType.HashLock)
        {
            SecretHash = secret;
            Hash = ComputeHash();
            Address = new Address(AddressType.HashLock, Hash.Bytes.SubArray(0, Address.RAW_SIZE - 1));
        }

        private HashLockHash ComputeHash()
        {
            using (var stream = new ByteStream())
            {
                stream.Write(this);
                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                return new HashLockHash(hasher.ComputeBytes(message).GetBytes());
            }
        }
    }

    public class SecretHash
    {
        public readonly SecretHashType Type;
        public readonly Hash256 Hash;

        public SecretHash(SecretHashType type, Hash256 hash)
        {
            Type = type;
            Hash = hash;
        }
    }

    public class Secret
    {
        public readonly byte[] Bytes;
        public const int SIZE = 32;

        public Secret(byte[] bytes)
        {
            Bytes = bytes;
        }

        public static Secret GenerateSecret()
        {
            var buffer = new byte[SIZE];
            new Random().NextBytes(buffer);
            return new Secret(buffer);
        }

        public SecretHash ComputeSecretHash(SecretHashType type)
        {
            var hasher = GetHasher(type);
            return new SecretHash(type, new Hash256(hasher.ComputeBytes(Bytes).GetBytes()));
        }

        private IHash GetHasher(SecretHashType type)
        {
            switch (type)
            {
                case SecretHashType.SHA3:
                    return HashFactory.Crypto.SHA3.CreateKeccak256();
                case SecretHashType.SHA256:
                    return HashFactory.Crypto.CreateSHA256();
                default:
                    throw new NotImplementedException();
            }
        }

        public bool Equals(Secret other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Bytes.IsEqual(other.Bytes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Secret) obj);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var b in Bytes)
                {
                    hash = hash * 23 + b;
                }
                return hash;
            }
        }
    }

    public enum SecretHashType : byte
    {
        SHA3 = 0,
        SHA256 = 1,
    }

    public class HashLockHash : TxDeclarationHash
    {
        public HashLockHash(byte[] bytes) : base(bytes) { }
    }

    public class TimeLockHash : TxDeclarationHash
    {
        public TimeLockHash(byte[] bytes) : base(bytes) { }
    }

    public class SecretRevelationHash : TxDeclarationHash
    {
        public SecretRevelationHash(byte[] bytes) : base(bytes) { }
    }
    
    public class TimeLock : TxAddressDeclaration
    {
        public readonly long Timestamp;
        public readonly TimeLockHash Hash;

        public TimeLock(long timestamp) : base(DeclarationType.TimeLock)
        {
            Timestamp = timestamp;
            Hash = ComputeHash();
            var buffer = new byte[Address.RAW_SIZE - 1];
            Array.Copy(BitConverter.GetBytes(timestamp), buffer, 8);
            Address = new Address(AddressType.TimeLock, buffer);
        }

        private TimeLockHash ComputeHash()
        {
            using (var stream = new ByteStream())
            {
                stream.Write(this);
                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                return new TimeLockHash(hasher.ComputeBytes(message).GetBytes());
            }
        }
    }
    
    public class SecretRevelation : TxDeclaration, IEquatable<SecretRevelation>
    {
        public readonly Secret Secret;
        public readonly SecretRevelationHash Hash;

        public SecretRevelation(Secret secret) : base(DeclarationType.Secret)
        {
            Secret = secret;
            Hash = ComputeHash();
        }

        private SecretRevelationHash ComputeHash()
        {
            using (var stream = new ByteStream())
            {
                stream.Write(this);
                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                return new SecretRevelationHash(hasher.ComputeBytes(message).GetBytes());
            }
        }

        public bool Equals(SecretRevelation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Secret.Equals(other.Secret);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SecretRevelation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Secret != null ? Secret.GetHashCode() : 0);
            }
        }
    }

    public class HistoricalTransaction
    {
        public readonly long LedgerHeight;
        public readonly long LedgerTimestamp;
        public readonly Transaction Transaction;

        public HistoricalTransaction(long ledgerHeight, Transaction transaction, long ledgerTimestamp)
        {
            LedgerHeight = ledgerHeight;
            Transaction = transaction;
            LedgerTimestamp = ledgerTimestamp;
        }
    }

    public class VendingMachine : TxAddressDeclaration
    {
        public readonly Address Owner;
        public readonly Currency CurrencyIn;
        public readonly Currency CurrencyOut;
        public readonly Amount Rate;

        public readonly VendingMachineHash Hash;

        public VendingMachine(Address owner, Currency @in, Currency @out, Amount rate) : base(DeclarationType.VendingMachine)
        {
            Owner = owner;
            CurrencyIn = @in;
            CurrencyOut = @out;
            Rate = rate;
            Hash = ComputeHash();
            Address = new Address(AddressType.VendingMachine, Hash.Bytes.SubArray(0, Address.RAW_SIZE - 1));
        }

        private VendingMachineHash ComputeHash()
        {
            using (var stream = new ByteStream())
            {
                stream.Write(this);
                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                return new VendingMachineHash(hasher.ComputeBytes(message).GetBytes());
            }
        }
    }

    public class VendingMachineHash : TxDeclarationHash
    {
        public VendingMachineHash(byte[] bytes) : base(bytes) { }
    }
}
