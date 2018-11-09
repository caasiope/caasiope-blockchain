using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caasiope.NBitcoin
{
    public class BitWriter
    {
        readonly List<bool> values = new List<bool>();
        public int Count => values.Count;

        public void Write(bool value)
        {
            values.Insert(Position, value);
            Position++;
        }

        public void Write(byte[] bytes)
        {
            Write(bytes, bytes.Length * 8);
        }

        public void Write(byte[] bytes, int bitCount)
        {
            bytes = SwapEndianBytes(bytes);
            var array = new BitArray(bytes);
            values.InsertRange(Position, array.OfType<bool>().Take(bitCount));
            Position += bitCount;
        }

        public byte[] ToBytes()
        {
            var array = ToBitArray();
            var bytes = ToByteArray(array);
            bytes = SwapEndianBytes(bytes);
            return bytes;
        }

        //BitArray.CopyTo do not exist in portable lib
        private static byte[] ToByteArray(BitArray bits)
        {
            var arrayLength = bits.Length / 8;
            if (bits.Length % 8 != 0)
                arrayLength++;
            var array = new byte[arrayLength];

            for (var i = 0; i < bits.Length; i++)
            {
                var b = i / 8;
                var offset = i % 8;
                array[b] |= bits.Get(i) ? (byte)(1 << offset) : (byte)0;
            }
            return array;
        }

        public BitArray ToBitArray()
        {
            return new BitArray(values.ToArray());
        }

        static byte[] SwapEndianBytes(byte[] bytes)
        {
            var output = new byte[bytes.Length];
            for (var i = 0; i < output.Length; i++)
            {
                byte newByte = 0;
                for (var ib = 0; ib < 8; ib++)
                {
                    newByte += (byte)(((bytes[i] >> ib) & 1) << (7 - ib));
                }
                output[i] = newByte;
            }
            return output;
        }

        public int Position { get; set; }

        public void Write(BitArray bitArray, int bitCount)
        {
            for (var i = 0; i < bitCount; i++)
            {
                Write(bitArray.Get(i));
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder(values.Count);
            for (var i = 0; i < Count; i++)
            {
                if (i != 0 && i % 8 == 0)
                    builder.Append(' ');
                builder.Append(values[i] ? "1" : "0");
            }
            return builder.ToString();
        }
    }
}