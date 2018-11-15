using Caasiope.Protocol.Types;
using HashLib;

namespace Caasiope.Protocol.MerkleTrees
{
    public static class HasherFactory
    {
        public static Hasher CreateHasher(Network network, long height)
        {
            if(network == ZodiacNetwork.Instance && height < 100)
                return new Hasher1();
            return new Hasher();
        }
    }

    public class Hasher
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
