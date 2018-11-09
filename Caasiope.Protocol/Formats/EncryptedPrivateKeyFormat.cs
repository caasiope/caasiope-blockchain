using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Caasiope.Protocol.Types;
using Caasiope.NBitcoin.Crypto;
using Caasiope.NBitcoin.DataEncoders;
using Caasiope.NBitcoin;
using Caasiope.NBitcoin.Crypto.Cryptsharp;

namespace Caasiope.Protocol.Formats
{
	// The format used to securly encrypted and store PrivateKey is derived from BIP38
    public static class EncryptedPrivateKeyFormat
	{
		private static readonly byte FLAG = 0x0C0;

		public static string Encrypt(PrivateKeyNotWallet key, string password, Network network)
        {
            var version = network.GetVersionBytes(Network.VersionByte.BIP38_ENCRYPTED_PRIVATE_KEY);
            var vch = key.PrivateKey.GetBytes();
            //Compute the Bitcoin address (ASCII),
            var addressBytes = Encoders.ASCII.DecodeData(key.PublicKey.GetAddress().Encoded);
            // and take the first four bytes of SHA256(SHA256()) of it. Let's call this "addresshash".
            var addresshash = Hashes.Hash256(addressBytes).ToBytes().SafeSubarray(0, 4);

            var derived = SCrypt.BitcoinComputeDerivedKey(Encoding.UTF8.GetBytes(password), addresshash);

            var encrypted = Bip38Engine.EncryptKey(vch, derived);
            
            //flagByte |= (key.IsCompressed ? (byte)0x20 : (byte)0x00); // WTF OR 0x00 is IDENTITY

            var bytes = version
				.Concat(new[] { FLAG })
                .Concat(addresshash)
                .Concat(encrypted).ToArray();
            return Encoders.Base58Check.EncodeData(bytes);
        }

        public static PrivateKeyNotWallet Decrypt(string encoded, string password, Network network)
        {
            var decoded = Encoders.Base58Check.DecodeData(encoded);

	        var version = decoded.SafeSubarray(0, 1);
			var flag = decoded.SafeSubarray(1, 1);
			var addresshash = decoded.SafeSubarray(2, 4);
	        var encrypted = decoded.SafeSubarray(6, 32);

	        var expected = network.GetVersionBytes(Network.VersionByte.BIP38_ENCRYPTED_PRIVATE_KEY);
			if (!Utils.ArrayEqual(version, expected))
		        throw new ArgumentException("Invalid version");
	        if (!Utils.ArrayEqual(flag, new[] { FLAG }))
		        throw new ArgumentException("Invalid flag");

			var derived = SCrypt.BitcoinComputeDerivedKey(Encoding.UTF8.GetBytes(password), addresshash);
            var keyBytes = Bip38Engine.DecryptKey(encrypted, derived);

            var key = PrivateKeyNotWallet.FromBytes(keyBytes);

            var addressBytes = Encoders.ASCII.DecodeData(key.PublicKey.GetAddress().Encoded);
            var salt = Hashes.Hash256(addressBytes).ToBytes().SafeSubarray(0, 4);

	        if(!Utils.ArrayEqual(salt, addresshash))
				throw new ArgumentException("Invalid password (or invalid Network)");

            return key;
        }
    }

    public static class Bip38Engine
    {
        internal static byte[] EncryptKey(byte[] key, byte[] derived)
        {
			Debug.Assert(key.Length == 32);
	        Debug.Assert(derived.Length == 64);
			var keyhalf1 = key.SafeSubarray(0, 16);
            var keyhalf2 = key.SafeSubarray(16, 16);
            return EncryptKey(keyhalf1, keyhalf2, derived);
        }

        private static byte[] EncryptKey(byte[] keyhalf1, byte[] keyhalf2, byte[] derived)
        {
            var derivedhalf1 = derived.SafeSubarray(0, 32);
            var derivedhalf2 = derived.SafeSubarray(32, 32);

            var encryptedhalf1 = new byte[16];
            var encryptedhalf2 = new byte[16];
#if USEBC || WINDOWS_UWP
			var aes = BitcoinEncryptedSecret.CreateAES256(true, derivedhalf2);
#else
            var aes = CreateAES256();
            aes.Key = derivedhalf2;
            var encrypt = aes.CreateEncryptor();
#endif

            for (int i = 0; i < 16; i++)
            {
                derivedhalf1[i] = (byte)(keyhalf1[i] ^ derivedhalf1[i]);
            }
#if USEBC || WINDOWS_UWP
			aes.ProcessBytes(derivedhalf1, 0, 16, encryptedhalf1, 0);
			aes.ProcessBytes(derivedhalf1, 0, 16, encryptedhalf1, 0);
#else
            encrypt.TransformBlock(derivedhalf1, 0, 16, encryptedhalf1, 0);
#endif
            for (int i = 0; i < 16; i++)
            {
                derivedhalf1[16 + i] = (byte)(keyhalf2[i] ^ derivedhalf1[16 + i]);
            }
#if USEBC || WINDOWS_UWP
			aes.ProcessBytes(derivedhalf1, 16, 16, encryptedhalf2, 0);
			aes.ProcessBytes(derivedhalf1, 16, 16, encryptedhalf2, 0);
#else
            encrypt.TransformBlock(derivedhalf1, 16, 16, encryptedhalf2, 0);
#endif
            return encryptedhalf1.Concat(encryptedhalf2).ToArray();
        }

        internal static byte[] DecryptKey(byte[] encrypted, byte[] derived)
        {
			Debug.Assert(encrypted.Length == 32);
			Debug.Assert(derived.Length == 64);
            var derivedhalf1 = derived.SafeSubarray(0, 32);
            var derivedhalf2 = derived.SafeSubarray(32, 32);

            var encryptedHalf1 = encrypted.SafeSubarray(0, 16);
            var encryptedHalf2 = encrypted.SafeSubarray(16, 16);

            byte[] bitcoinprivkey1 = new byte[16];
            byte[] bitcoinprivkey2 = new byte[16];

#if USEBC || WINDOWS_UWP
			var aes = CreateAES256(false, derivedhalf2);
			aes.ProcessBytes(encryptedHalf1, 0, 16, bitcoinprivkey1, 0);
			aes.ProcessBytes(encryptedHalf1, 0, 16, bitcoinprivkey1, 0);
#else
            var aes = CreateAES256();
            aes.Key = derivedhalf2;
            var decrypt = aes.CreateDecryptor();
            //Need to call that two time, seems AES bug
            decrypt.TransformBlock(encryptedHalf1, 0, 16, bitcoinprivkey1, 0);
            decrypt.TransformBlock(encryptedHalf1, 0, 16, bitcoinprivkey1, 0);
#endif



            for (int i = 0; i < 16; i++)
            {
                bitcoinprivkey1[i] ^= derivedhalf1[i];
            }
#if USEBC || WINDOWS_UWP
			aes.ProcessBytes(encryptedHalf2, 0, 16, bitcoinprivkey2, 0);
			aes.ProcessBytes(encryptedHalf2, 0, 16, bitcoinprivkey2, 0);
#else
            //Need to call that two time, seems AES bug
            decrypt.TransformBlock(encryptedHalf2, 0, 16, bitcoinprivkey2, 0);
            decrypt.TransformBlock(encryptedHalf2, 0, 16, bitcoinprivkey2, 0);
#endif
            for (int i = 0; i < 16; i++)
            {
                bitcoinprivkey2[i] ^= derivedhalf1[16 + i];
            }

            return bitcoinprivkey1.Concat(bitcoinprivkey2).ToArray();
        }

#if USEBC || WINDOWS_UWP
		internal static PaddedBufferedBlockCipher CreateAES256(bool encryption, byte[] key)
		{
			var aes = new PaddedBufferedBlockCipher(new AesFastEngine(), new Pkcs7Padding());
			aes.Init(encryption, new KeyParameter(key));
			aes.ProcessBytes(new byte[16], 0, 16, new byte[16], 0);
			return aes;
		}
#else
        internal static Aes CreateAES256()
        {
            var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.ECB;
            aes.IV = new byte[16];
            return aes;
        }
#endif
    }
}
