using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class HashLock : TxAddressDeclaration
    {
        public SecretHash SecretHash;

        public HashLock(SecretHash secretHash, string address) : this()
        {
            SecretHash = secretHash;
            Address = address;
        }

        public HashLock() : base((byte)DeclarationType.HashLock) { }
    }

    public class SecretHash
    {
        public readonly SecretHashType Type;
        public readonly string Hash;

        public SecretHash(SecretHashType type, string hash)
        {
            Type = type;
            Hash = hash;
        }
    }
}