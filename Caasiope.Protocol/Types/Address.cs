using System;
using System.Diagnostics;
using Caasiope.Protocol.Formats;
using Helios.Common.Extensions;

namespace Caasiope.Protocol.Types
{
    public enum AddressType : byte
    {
        ECDSA = 0x1,
        MultiSignatureECDSA = 0x2,
        HashLock = 0x3,
        TimeLock = 0x4,
    }

    [DebuggerDisplay("{Encoded}")]
    public class Address
    {
        // private byte[] bytes;
        public readonly string Encoded;

        public readonly AddressType Type;
        private readonly byte[] hash;
        public const int RAW_SIZE = 21;
        public const int ENCODED_SIZE = 40;

        public Address(string encoded)
        {
            Debug.Assert(encoded.Length == ENCODED_SIZE);
            Encoded = encoded;
            hash = Address32Format.Decode(encoded, out Type);
            Debug.Assert(hash != null);
            Debug.Assert(hash.Length == RAW_SIZE - 1);
        }

        public Address(AddressType type, byte[] hash)
        {
            Debug.Assert(hash.Length == RAW_SIZE - 1);
            this.hash = hash;
            this.Type = type;
            Encoded = Address32Format.Encode(type, hash);
            Debug.Assert(Encoded != null);
        }

        // TODO move this to a format class
        public static Address FromRawBytes(byte[] bytes)
        {
            Debug.Assert(bytes.Length == RAW_SIZE);
            return new Address((AddressType) bytes[0], bytes.SubArray(1, bytes.Length - 1));
        }

        public byte[] ToRawBytes()
        {
            var length = hash.Length;
            var buffer = new byte[length + 1];
            buffer[0] = (byte) Type;
            Array.Copy(hash, 0, buffer, 1, length);
            return buffer;
        }

        public static bool operator == (Address a, Address b)
        {
            if ((object)a == null || (object)b == null)
                return ((object)a == null && (object)b == null);
            return a.Encoded == b.Encoded;
        }

        public static bool operator != (Address a, Address b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Encoded.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Address);
        }

        public virtual bool Equals(Address other)
        {
            if (this == other)
                return true;
            if (ReferenceEquals(null, other))
                return false;
            return this == other;
        }
    }
}