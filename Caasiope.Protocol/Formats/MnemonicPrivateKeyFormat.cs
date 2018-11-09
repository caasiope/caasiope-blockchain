using System.Diagnostics;
using Caasiope.Protocol.Types;
using Caasiope.NBitcoin.BIP39;
using Caasiope.NBitcoin.DataEncoders;

namespace Caasiope.Protocol.Formats
{
    public static class MnemonicPrivateKeyFormat
    {
        public static PrivateKey FromMnemonic(string mnemonicPhrase)
        {
            var mnemonic = new Mnemonic(mnemonicPhrase);
            Debug.Assert(mnemonic.Words.Length == 24);
            return new PrivateKey(mnemonic.DeriveData());
        }

        public static PrivateKey FromMnemonicEncrypted(string mnemonic, string password, Network network)
        {
            var mnem = new Mnemonic(mnemonic);
            Debug.Assert(mnem.Words.Length == 28);
            var encrypted = Encoders.Base58Check.EncodeData(mnem.DeriveData());
            return EncryptedPrivateKeyFormat.Decrypt(encrypted, password, network);
        }

        public static string GetMnemonic(PrivateKeyNotWallet key, Wordlist wordlist)
        {
            var mnemonic = new Mnemonic(wordlist, key.PrivateKey.GetBytes());
            Debug.Assert(mnemonic.Words.Length == 24);
            return mnemonic.ToString();
        }

        public static string GetMnemonicFromEncrypted(PrivateKeyNotWallet key, Wordlist wordlist, string password, Network network)
        {
            var encryptedString = EncryptedPrivateKeyFormat.Encrypt(key, password, network);
            var encrypted = Encoders.Base58Check.DecodeData(encryptedString);
            var mnemonic = new Mnemonic(wordlist, encrypted);
            Debug.Assert(mnemonic.Words.Length == 28);
            return mnemonic.ToString();
        }
    }
}