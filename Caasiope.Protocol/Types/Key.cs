using System;
using System.Diagnostics;
using Helios.Common.Extensions;
using Caasiope.NBitcoin.Crypto;

namespace Caasiope.Protocol.Types
{
    // TODO maybe a bad idea
    [DebuggerDisplay("{ToBase64()}")]
    public abstract class Key : IEquatable<Key>
    {
        protected ECDSAKey ECDSAKey;
        protected readonly byte[] vch; // used to export the ECDSAKey
        protected Key(byte[] data, bool isPrivateKey)
        {
            vch = data;
            ECDSAKey = new ECDSAKey(data, isPrivateKey);
        }

        public string ToBase64()
        {
            return Convert.ToBase64String(vch);
        }

        public static bool operator == (Key key1, Key key2)
        {
            return key1.vch.IsEqual(key2.vch);
        }

        public static bool operator !=(Key key1, Key key2)
        {
            return !(key1 == key2);
        }

        public byte[] GetBytes()
        {
            return vch;
        }

        public bool Equals(Key other)
        {
            return vch.IsEqual(other.vch);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Key))
                return false;

            return Equals((Key) obj);
        }
        
        public override int GetHashCode()
        {
            // TODO maybe XOR each byte
            return vch.Length;
        }
    }
}