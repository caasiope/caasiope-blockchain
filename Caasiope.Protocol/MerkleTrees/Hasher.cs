using Caasiope.NBitcoin;
using Caasiope.Protocol.Types;
using HashLib;

namespace Caasiope.Protocol.MerkleTrees
{
    public interface IHasher<in T>
    {
        Hash256 GetHash(T item); // we should try to consider this is expensive
    }

    public static class HasherFactory
    {
        public static Hasher CreateHasher(ProtocolVersion version)
        {
            if(version == ProtocolVersion.InitialVersion)
                return new Hasher1();
            return new Hasher();
        }
    }

    public class Hasher : IHasher<Account>
    {
        // used only for merkle tree
        internal virtual AccountHash GetHash(Account account)
        {
            using (var stream = new ByteStream())
            {
                stream.Write(account);

                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                var hash = hasher.ComputeBytes(message).GetBytes();
                return new AccountHash(hash);
            }
        }
        
        Hash256 IHasher<Account>.GetHash(Account item)
        {
            return GetHash(item);
        }
    }

    public class Hasher1 : Hasher
    {
        internal override AccountHash GetHash(Account account)
        {
            using (var stream = new ByteStream())
            {
                stream.Write1(account);

                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                var hash = hasher.ComputeBytes(message).GetBytes();
                return new AccountHash(hash);
            }
        }
    }
}
