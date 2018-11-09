using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Caasiope.Protocol.Formats
{
    public static class Bech32
    {
        // used for polymod
        private static readonly uint[] generator = { 0x3b6a57b2, 0x26508e6d, 0x1ea119fa, 0x3d4233dd, 0x2a1462b3 };

        // charset is the sequence of ascii characters that make up the bech32
        // alphabet.  Each character represents a 5-bit squashed byte.
        // q = 0b00000, p = 0b00001, z = 0b00010, and so on.

        private const string charset = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";

        // icharset is a mapping of 8-bit ascii characters to the charset
        // positions.  Both uppercase and lowercase ascii are mapped to the 5-bit
        // position values.
        private static readonly short[] icharset =
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            15, -1, 10, 17, 21, 20, 26, 30, 7, 5, -1, -1, -1, -1, -1, -1,
            -1, 29, -1, 24, 13, 25, 9, 8, 23, -1, 18, 22, 31, 27, 19, -1,
            1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, -1, -1, -1, -1, -1,
            -1, 29, -1, 24, 13, 25, 9, 8, 23, -1, 18, 22, 31, 27, 19, -1,
            1, 0, 3, 16, 11, 28, 12, 14, 6, 4, 2, -1, -1, -1, -1, -1
        };

        // PolyMod takes a byte slice and returns the 32-bit BCH checksum.
        // Note that the input bytes to PolyMod need to be squashed to 5-bits tall
        // before being used in this function.  And this function will not error,
        // but instead return an unsuable checksum, if you give it full-height bytes.
        private static uint PolyMod(byte[] values)
        {
            uint chk = 1;
            foreach (byte value in values)
            {
                var top = chk >> 25;
                chk = (chk & 0x1ffffff) << 5 ^ value;
                for (var i = 0; i < 5; ++i)
                {
                    if (((top >> i) & 1) == 1)
                    {
                        chk ^= generator[i];
                    }
                }
            }
            return chk;
        }


        // on error, return null;
        public static byte[] Decode(string encoded)
        {
            var squashed = DecodeSquashed(encoded);
            if (squashed == null)
            {
                return null;
            }
            return Bytes5to8(squashed);
        }

        // on error, data == null
        internal static byte[] DecodeSquashed(string adr)
        {
            if (adr.Length < 7)
            {
                Debug.WriteLine("separator not present in address");
                return null;
            }

            adr = CheckAndFormat(adr);
            if (adr == null)
            {
                return null;
            }

            // get squashed data
            var squashed = StringToSquashedBytes(adr);
            if (squashed == null)
            {
                return null;
            }

            // make sure checksum works
            if (!VerifyChecksum(squashed))
            {
                Debug.WriteLine("Checksum invalid");
                return null;
            }

            // chop off checksum to return only payload
            var length = squashed.Length - 6;
            var data = new byte[length];
            Array.Copy(squashed, 0, data, 0, length);
            return data;
        }

        // on error, return null
        private static string CheckAndFormat(string adr)
        {
            // make an all lowercase and all uppercase version of the input string
            var lowAdr = adr.ToLower();
            var highAdr = adr.ToUpper();

            // if there's mixed case, that's not OK
            if (adr != lowAdr && adr != highAdr)
            {
                Debug.WriteLine("mixed case address");
                return null;
            }

            // default to lowercase
            return lowAdr;
        }

        private static bool VerifyChecksum(byte[] data)
        {
            var checksum = PolyMod(data);
            // make sure it's 1 (from the LSB flip in CreateChecksum
            return checksum == 1;
        }

        // on error, return null
        private static byte[] StringToSquashedBytes(string input)
        {
            byte[] squashed = new byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                var buffer = icharset[c];
                if (buffer == -1)
                {
                    Debug.WriteLine("contains invalid character " + c);
                    return null;
                }
                squashed[i] = (byte)buffer;
            }

            return squashed;
        }

        // we encode the data and the header
        public static string Encode(byte[] data)
        {
            var squashed = Bytes8to5(data);
            if (squashed == null)
                return string.Empty;

            return EncodeSquashed(squashed);
        }

        // on error, return null
        internal static string EncodeSquashed(byte[] data)
        {
            var checksum = CreateChecksum(data);
            var combined = data.Concat(checksum).ToArray();

            // Should be squashed, return empty string if it's not.
            var encoded = SquashedBytesToString(combined);
            return encoded;
        }

        private static byte[] CreateChecksum(byte[] data)
        {
            // put 6 zero bytes on at the end
            var values = data.Concat(new byte[6]).ToArray();
            //get checksum for whole slice

            // flip the LSB of the checksum data after creating it
            var checksum = PolyMod(values) ^ 1;

            byte[] ret = new byte[6];
            for (var i = 0; i < 6; i++)
            {
                // note that this is NOT the same as converting 8 to 5
                // this is it's own expansion to 6 bytes from 4, chopping
                // off the MSBs.
                ret[i] = (byte)(checksum >> (5 * (5 - i)) & 0x1f);
            }

            return ret;
        }

        private static string SquashedBytesToString(byte[] input)
        {
            string s = string.Empty;
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if ((c & 0xe0) != 0)
                {
                    Debug.WriteLine("high bits set at position {0}: {1}", i, c);
                    return null;
                }
                s += charset[c];
            }

            return s;
        }

        public static byte[] Bytes8to5(byte[] data)
        {
            return ByteSquasher(data, 8, 5);
        }

        public static byte[] Bytes5to8(byte[] data)
        {
            return ByteSquasher(data, 5, 8);
        }

        // ByteSquasher squashes full-width (8-bit) bytes into "squashed" 5-bit bytes,
        // and vice versa.  It can operate on other widths but in this package only
        // goes 5 to 8 and back again.  It can return null if the squashed input
        // you give it isn't actually squashed, or if there is padding (trailing q characters)
        // when going from 5 to 8
        private static byte[] ByteSquasher(byte[] input, int inputWidth, int outputWidth)
        {
            int bitstash = 0;
            int accumulator = 0;
            var output = new List<byte>();
            var maxOutputValue = (1 << outputWidth) - 1;

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c >> inputWidth != 0)
                {
                    Debug.WriteLine("byte {0} ({1}) high bits set", i, c);
                    return null;
                }
                accumulator = (accumulator << inputWidth) | c;
                bitstash += inputWidth;
                while (bitstash >= outputWidth)
                {
                    bitstash -= outputWidth;
                    output.Add((byte)((accumulator >> bitstash) & maxOutputValue));
                }
            }

            // pad if going from 8 to 5
            if (inputWidth == 8 && outputWidth == 5)
            {
                if (bitstash != 0)
                {
                    output.Add((byte)(accumulator << (outputWidth - bitstash) & maxOutputValue));
                }
            }
            else if (bitstash >= inputWidth || ((accumulator << (outputWidth - bitstash)) & maxOutputValue) != 0)
            {
                // no pad from 5 to 8 allowed
                Debug.WriteLine("invalid padding from {0} to {1} bits", inputWidth, outputWidth);
                return null;
            }
            return output.ToArray();
        }
    }
}