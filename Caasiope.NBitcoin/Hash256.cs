using System;
using System.Diagnostics;
using Caasiope.NBitcoin.BouncyCastle.util;

namespace Caasiope.NBitcoin
{
    [DebuggerDisplay("{ToBase64()}")]
    public class Hash256 : IComparable<Hash256>
    {
        public readonly byte[] Bytes;
        public const int SIZE = 32;

        public Hash256(byte[] bytes)
        {
            Debug.Assert(bytes.Length == SIZE);
            Bytes = bytes;
        }

        public static Hash256 Zero = new Hash256(new byte[SIZE]);

        public override bool Equals(object obj)
        {
            var o = obj as Hash256;
            return o != null && Bytes.Compare(o.Bytes);
        }

        public override int GetHashCode()
        {
            return Arrays.GetHashCode(Bytes);
        }

        public byte[] ToBytes()
        {
            return Bytes;
        }

        public string ToBase64()
        {
            return Convert.ToBase64String(Bytes, 0, Bytes.Length);
        }

        public int CompareTo(Hash256 other)
        {
            // Assume it's two big numbers, so we compare digit numbers
            var result = 0;
            for (var index = 0; index < SIZE; index++)
            {
                result = Bytes[index].CompareTo(other.Bytes[index]);
                if (result != 0) return result;
            }
            return result;
        }
    }
}
