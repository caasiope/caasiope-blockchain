using System.Diagnostics;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;

namespace Caasiope.Protocol.Formats
{
    public static class Address32Format
    {
        public static string Encode(AddressType type, byte[] data)
        {
#if DEBUG
            var encoded = EncodeDebug(type, data);
            AddressType t;
            Debug.Assert(DecodeDebug(encoded, out t).IsEqual(data) && t == type);
            return encoded;
        }

        public static string EncodeDebug(AddressType type, byte[] data)
        {
#endif
            var header = Bech32.Bytes8to5( new []{ (byte)type });
            var body = Bech32.Bytes8to5(data);
            return Bech32.EncodeSquashed((header.Append(body)));
        }

        public static byte[] Decode(string encoded, out AddressType type)
        {
#if DEBUG
            var decoded = DecodeDebug(encoded, out type);
            Debug.Assert(EncodeDebug(type, decoded) == encoded);
            return decoded;
        }

        public static byte[] DecodeDebug(string encoded, out AddressType type)
        {
#endif
            var squashed = Bech32.DecodeSquashed(encoded);
            type = (AddressType) Bech32.Bytes5to8(squashed.SubArray(0, 2))[0];
            return Bech32.Bytes5to8(squashed.SubArray(2, squashed.Length - 2));
        }
    }
}
