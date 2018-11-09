namespace Caasiope.Protocol.Types
{
    public class HashLockAccount
    {
        public readonly Address Address;
        public readonly SecretHash SecretHash;

        public HashLockAccount(Address address, SecretHash secret)
        {
            Address = address;
            SecretHash = secret;
        }
    }
}