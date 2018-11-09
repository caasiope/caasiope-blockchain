namespace Caasiope.Protocol.Types
{
    // TODO find a real name
    public class PrivateKeyNotWallet
    {
        public readonly PrivateKey PrivateKey;
        public readonly PublicKey PublicKey;
        public readonly Account Account;

        private PrivateKeyNotWallet(PrivateKey key)
        {
            PrivateKey = key;
            PublicKey = key.GetPublicKey();
            Account = Account.FromAddress(PublicKey.GetAddress());
        }

        public static PrivateKeyNotWallet FromBase64(string key)
        {
            return new PrivateKeyNotWallet(PrivateKey.FromBase64(key));
        }

        public static PrivateKeyNotWallet FromBytes(byte[] bytes)
        {
            return new PrivateKeyNotWallet(new PrivateKey(bytes));
        }

        public static implicit operator PrivateKey(PrivateKeyNotWallet currency)
        {
            return currency.PrivateKey;
        }

        public static implicit operator PublicKey(PrivateKeyNotWallet currency)
        {
            return currency.PublicKey;
        }

        public static implicit operator Address(PrivateKeyNotWallet currency)
        {
            return currency.Account.Address;
        }
    }
}