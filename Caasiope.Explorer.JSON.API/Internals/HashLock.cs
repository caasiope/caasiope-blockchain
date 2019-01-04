using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class HashLock : TxDeclaration
    {
        public SecretHash SecretHash;

        public HashLock(SecretHash secretHash) : this()
        {
            SecretHash = secretHash;
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