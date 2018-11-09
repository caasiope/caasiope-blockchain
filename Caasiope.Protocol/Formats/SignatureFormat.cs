using System.Diagnostics;
using System.IO;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Caasiope.NBitcoin;
using Caasiope.NBitcoin.BouncyCastle.math;
using Caasiope.NBitcoin.Crypto;

namespace Caasiope.Protocol.Formats
{
    public static class SignatureFormat
    {
        public static SignatureByte ToBytes(ECDSASignature signature)
        {
            using (var stream = new MemoryStream(65))
            {
                byte overflow = 0;
                bool isOverflow;

                var r = ToByteArray(signature.R, out isOverflow);
                if(isOverflow)
                    overflow |= 0x1;
                Debug.Assert(signature.R.Equals(ToInteger(r, isOverflow)));

                var s = ToByteArray(signature.S, out isOverflow);
                if (isOverflow)
                    overflow |= 0x2;
                Debug.Assert(signature.S.Equals(ToInteger(s, isOverflow)));

                stream.WriteByte(overflow);
                stream.Write(r, 0, 32);
                stream.Write(s, 0, 32);
			    return new SignatureByte(stream.ToArray());
            }
        }

        private static BigInteger ToInteger(byte[] bytes, bool isOverflow)
        {
            if (isOverflow)
            {
                bytes = new byte[] {0}.Append(bytes);
            }
            return new BigInteger(bytes);
        }

        private static byte[] ToByteArray(BigInteger integer, out bool isOverflow)
        {
            var bytes = integer.ToByteArray();
            Debug.Assert(bytes.Length >= 30 && bytes.Length <= 33);
            if (bytes.Length == 33)
            {
                Debug.Assert(bytes[0] == 0);
                isOverflow = true;
                return bytes.SubArray(1, 32);
            }
            if (bytes.Length < 32)
            {
                isOverflow = false;
                return new byte[32-bytes.Length].Append(bytes);
            }
            isOverflow = false;
            return bytes;
        }

        public static ECDSASignature FromBytes(SignatureByte bytes)
        {
            using (var stream = new MemoryStream(bytes.Bytes))
            {
                var overflow = stream.ReadByte();
                var r = ToInteger(stream.ReadBytes(32), (overflow & 0x1) == 0x1 );
                var s = ToInteger(stream.ReadBytes(32), (overflow & 0x2) == 0x2 );
                return new ECDSASignature(r, s);
            }
        }
    }
}
