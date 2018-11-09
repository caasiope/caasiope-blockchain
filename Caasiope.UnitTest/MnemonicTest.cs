using System;
using Caasiope.Protocol.Formats;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caasiope.NBitcoin.BIP39;

namespace Caasiope.UnitTest
{
    [TestClass]
    public class MnemonicTest : TestBase
    {
        [TestMethod]
        public void TestMnemonic_CreateFromEncrypted_RestoreFromEncrypted()
        {
            foreach (var key in keys)
            {
                var account = PrivateKeyNotWallet.FromBase64(key);

                var password = "PassWoRd!";

                var words = MnemonicPrivateKeyFormat.GetMnemonicFromEncrypted(account, Wordlist.English, password, Network);
                var restored = MnemonicPrivateKeyFormat.FromMnemonicEncrypted(words, password, Network);
                Assert.IsTrue(account.PrivateKey.Equals(restored));
            }
        }

        public void TestMnemonicRestore_Language(Wordlist wordlist)
        {
            foreach (var key in keys)
            {
                var account = PrivateKeyNotWallet.FromBase64(key);

                var words = MnemonicPrivateKeyFormat.GetMnemonic(account, wordlist);
                var restored = MnemonicPrivateKeyFormat.FromMnemonic(words);
                Assert.IsTrue(account.PrivateKey.Equals(restored));
            }
        }

        [TestMethod]
        public void MnemonicRestore_English_Test()
        {
            TestMnemonicRestore_Language(Wordlist.English);
        }

        [TestMethod]
        public void MnemonicRestore_Japanese_Test()
        {
            TestMnemonicRestore_Language(Wordlist.Japanese);
        }

        [TestMethod]
        public void MnemonicRestore_ChineseSimplified_Test()
        {
            TestMnemonicRestore_Language(Wordlist.ChineseSimplified);
        }

        [TestMethod]
        public void MnemonicRestore_ChineseTraditional_Test()
        {
            TestMnemonicRestore_Language(Wordlist.ChineseTraditional);
        }

        [TestMethod]
        public void MnemonicRestore_French_Test()
        {
            TestMnemonicRestore_Language(Wordlist.French);
        }

        [TestMethod]
        public void MnemonicRestore_PortugueseBrazil_Test()
        {
            TestMnemonicRestore_Language(Wordlist.PortugueseBrazil);
        }

        [TestMethod]
        public void MnemonicRestore_Spanish_Test()
        {
            TestMnemonicRestore_Language(Wordlist.Spanish);
        }


        public void MnemonicRestoreEncrypted_Language(Wordlist wordlist)
        {
            var random = new Random();
            var encryptedKey = new byte[38]; // Encrypted key length is 38 bytes

            for (var i = 0; i < 10; i++)
            {
                random.NextBytes(encryptedKey);

                var mnemonic = new Mnemonic(wordlist, encryptedKey);

                Assert.IsTrue(mnemonic.Words.Length == 28);

                var restoredMnemonic = new Mnemonic(mnemonic.ToString());

                var restored = restoredMnemonic.DeriveData();

                Assert.IsTrue(encryptedKey.IsEqual(restored));
            }
        }

        [TestMethod]
        public void MnemonicRestoreEncrypted_English_Test()
        {
            MnemonicRestoreEncrypted_Language(Wordlist.English);
        }

        [TestMethod]
        public void MnemonicRestoreEncrypted_Japanese_Test()
        {
            MnemonicRestoreEncrypted_Language(Wordlist.Japanese);
        }

        [TestMethod]
        public void MnemonicRestoreEncrypted_ChineseSimplified_Test()
        {
            MnemonicRestoreEncrypted_Language(Wordlist.ChineseSimplified);
        }

        [TestMethod]
        public void MnemonicRestoreEncrypted_ChineseTraditional_Test()
        {
            MnemonicRestoreEncrypted_Language(Wordlist.ChineseTraditional);
        }

        [TestMethod]
        public void MnemonicRestoreEncrypted_French_Test()
        {
            MnemonicRestoreEncrypted_Language(Wordlist.French);
        }

        [TestMethod]
        public void MnemonicRestoreEncrypted_PortugueseBrazil_Test()
        {
            MnemonicRestoreEncrypted_Language(Wordlist.PortugueseBrazil);
        }

        [TestMethod]
        public void MnemonicRestoreEncrypted_Spanish_Test()
        {
            MnemonicRestoreEncrypted_Language(Wordlist.Spanish);
        }
    }
}