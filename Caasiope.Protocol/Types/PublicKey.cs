using System;
using System.Diagnostics;
using Caasiope.Protocol.Formats;
using HashLib;
using Caasiope.NBitcoin;

namespace Caasiope.Protocol.Types
{
    public class PublicKey : Key, IComparable<PublicKey>
    {
        public const int SIZE = 65;

        public PublicKey(byte[] data) : base(data, false)
        {
            Debug.Assert(data.Length == SIZE);
        }

        // TODO put in extension method
        public Address GetAddress()
        {
            var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
            var hash = hasher.ComputeBytes(vch).GetBytes();

            // take the last 20 bytes;
            var truncated = new byte[20];
            Array.Copy(hash, hash.Length - 20, truncated, 0, 20);

            return new Address(AddressType.ECDSA, truncated);
        }

        public bool CheckAddress(string encoded)
        {
            return encoded == GetAddress().Encoded;
        }

        public bool Verify(Hash256 hash, SignatureByte signature, Network network)
        {
            var data = SignatureEngine.FormatMessageForSigning(hash.Bytes, network);
            return ECDSAKey.Verify(data, SignatureFormat.FromBytes(signature));
        }

        public int CompareTo(PublicKey other)
        {
            // Assume it's two big numbers, so we compare digit numbers
            var result = 0;
            for (var index = 0; index < SIZE; index++)
            {
                result = vch[index].CompareTo(other.vch[index]);
                if (result != 0) return result;
            }
            return result;
        }
    }
}