using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caasiope.NBitcoin;
using Caasiope.Protocol.Formats;
using Caasiope.Protocol.Types;

namespace Caasiope.UnitTest
{
	[TestClass]
	public class EncryptedPrivateKeyTests : TestBase
	{
		[TestMethod]
		public void TestEncryptDecrypt()
		{
			foreach (var key in keys)
			{
				var wallet = PrivateKeyNotWallet.FromBase64(key);
				EncryptDecrypt(wallet, "I am the real S4t0sh1 !");
				EncryptDecrypt(wallet, "O RLY ?");
				EncryptDecrypt(wallet, "Just kidding :D");
				EncryptDecrypt(wallet, "What a @#$@$#!#$!#$@!#@#!^$^@ !");
				EncryptDecrypt(wallet, "乱七八糟");
			}
		}

		private void EncryptDecrypt(PrivateKeyNotWallet wallet, string password)
		{
			var network = TestNetwork.Instance;
			var encrypted = EncryptedPrivateKeyFormat.Encrypt(wallet, password, network);
			var decrypted = EncryptedPrivateKeyFormat.Decrypt(encrypted, password, network);
			Assert.IsTrue(Utils.ArrayEqual(decrypted.PrivateKey.GetBytes(), wallet.PrivateKey.GetBytes()));
		}
	}
}