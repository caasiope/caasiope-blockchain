using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class HashLock : TxDeclaration
    {
        public readonly SecretHash SecretHash;

        public HashLock(SecretHash secretHash)
        {
            SecretHash = secretHash;
        }
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