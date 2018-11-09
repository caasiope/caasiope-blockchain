using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caasiope.NBitcoin.Crypto;
using Caasiope.Protocol;
using Caasiope.Protocol.Formats;
using Caasiope.Protocol.Types;

namespace Caasiope.UnitTest
{
    [TestClass]
    public class SignatureTests : TestBase
    {
        [TestMethod]
        public void TestSignature()
        {
            // get the private key
            foreach (var base64 in keys)
            {
                PrivateKey key = PrivateKey.FromBase64(base64);
                Assert.AreEqual(base64, key.ToBase64());

                // derive the public key
                var pub = key.GetPublicKey();

                // encode the address
                var address = pub.GetAddress();

                var text = "Guillaume is the best !";
                // sign text
                var signed = key.SignMessage(text, Network);

                // check signature against public key
                Assert.IsTrue(pub.CheckSignature(text, signed, Network));

                // check public key against address
                Assert.IsTrue(pub.CheckAddress(address.Encoded));
            }
        }

        [TestMethod]
        public void TestSignatureVerify()
        {
            // get the private key
            foreach (var key in keys)
            {
                var account = PrivateKeyNotWallet.FromBase64(key);

                var text = "Guillaume is the best !";
                var hash = Hashes.Hash256(Encoding.UTF8.GetBytes(text));


                var signature = account.CreateSignature(hash, Network);
                Assert.IsTrue(signature.PublicKey.Verify(hash, signature.SignatureByte, Network));
            }
        }

        [TestMethod]
        public void TestAddress32Format()
        {
            // Private 7r7oFxKhhaH7UvMLpUXlcIEk0WWx7i4nw6BVnrKCmLk=
            var address = "qyl68tygnjx6qqwrsmynmejmc9wxlw7almv3397j";
            var hash = Address32Format.Decode(address, out var type);
            var encoded2 = Address32Format.Encode(type, hash);

            Assert.IsTrue(address == encoded2);
        }
        
        [TestMethod]
        public void TestTransaction()
        {

            // sign transaction
            var signed = new SignedTransaction(this.CreateExchangeTransaction(out var account1, out var account2));
            var hash = signed.Hash;

            signed.AddSignature(account1.CreateSignature(hash, Network));
            signed.AddSignature(account2.CreateSignature(hash, Network));

            // check transaction signature
            Assert.IsTrue(CheckSignatures(signed));
        }

        private Transaction CreatePaymentTransaction(out PrivateKeyNotWallet account1, out PrivateKeyNotWallet account2)
        {
            account1 = CreateAccount();
            account2 = CreateAccount();

            var declarations = new List<TxDeclaration>();

            var inputs = new List<TxInput>()
            {
                new TxInput(account1, Currency.BTC, Amount.FromWholeValue(10)),
            };

            var outputs = new List<TxOutput>()
            {
                    new TxOutput(account2, Currency.BTC, Amount.FromWholeValue(10)),
            };

            return new Transaction(declarations, inputs, outputs, TransactionMessage.Empty, DateTime.UtcNow.Ticks);
        }

        [TestMethod]
        public void TestMultipleTimesSameSignature()
        {

            // sign transaction
            var signed = new SignedTransaction(this.CreateExchangeTransaction(out var account1, out var account2));
            var hash = signed.Hash;

            signed.AddSignature(account1.CreateSignature(hash, Network));
            Assert.IsFalse(signed.AddSignature(account1.CreateSignature(hash, Network)));

            Assert.IsFalse(CheckSignatures(signed));

            signed.AddSignature(account2.CreateSignature(hash, Network));
            Assert.IsTrue(CheckSignatures(signed));
        }

        [TestMethod]
        public void TestWrongAccountSignature()
        {

            // sign transaction
            var signed = new SignedTransaction(CreatePaymentTransaction(out var account1, out var account2));
            var hash = signed.Hash;

            signed.AddSignature(account2.CreateSignature(hash, Network));
            Assert.IsFalse(CheckSignatures(signed));

            // we dont accept useless signatures
            signed.AddSignature(account1.CreateSignature(hash, Network));
            Assert.IsFalse(CheckSignatures(signed));

            signed.Signatures.Clear();
            // add only required signature
            signed.AddSignature(account1.CreateSignature(hash, Network));
            Assert.IsTrue(CheckSignatures(signed));
        }

        [TestMethod]
        public void TestMultiSignature()
        {
            var signed = new SignedTransaction(CreateMultiSignatureTransaction(out var account1, out var account2, out var account3, out var account4));
            var hash = signed.Hash;

            // output address -> useless
            signed.AddSignature(account4.CreateSignature(hash, Network));
            Assert.IsFalse(CheckSignatures(signed));

            // 1 out of 2 -> KO
            signed.AddSignature(account1.CreateSignature(hash, Network));
            Assert.IsFalse(CheckSignatures(signed));

            // 2 out of 2 -> OK
            signed.AddSignature(account2.CreateSignature(hash, Network));
            Assert.IsTrue(CheckSignatures(signed));

            // 3 out of 2 -> OK
            signed.AddSignature(account3.CreateSignature(hash, Network));
            Assert.IsTrue(CheckSignatures(signed));
        }

        [TestMethod]
        public void TestCurrency()
        {
            var currencies = new[] {"AAA", "ZZZ", "BTC", "LTC"};

            foreach (var currency in currencies)
            {
                var value = Currency.FromSymbol(currency);
                var name = Currency.ToSymbol(value);
                Assert.AreEqual(currency, name);
            }
        }

        [TestMethod]
        public void TestSignatureFormat()
        {
            Assert.Inconclusive();
        }

        public Transaction CreateMultiSignatureTransaction(out PrivateKeyNotWallet account1, out PrivateKeyNotWallet account2, out PrivateKeyNotWallet account3, out PrivateKeyNotWallet account4)
        {
            account1 = CreateAccount();
            account2 = CreateAccount();
            account3 = CreateAccount();
            account4 = CreateAccount();

            var multi = new MultiSignature(new List<Address> {account1, account2, account3}, 2);

            var declarations = new List<TxDeclaration>()
            {
                multi
            };

            var inputs = new List<TxInput>()
            {
                    new TxInput(multi.Address, Currency.BTC, Amount.FromWholeValue(10)),
            };

            var outputs = new List<TxOutput>()
            {
                    new TxOutput(account4, Currency.BTC, Amount.FromWholeValue(10)),
            };

            return new Transaction(declarations, inputs, outputs, TransactionMessage.Empty, DateTime.UtcNow.Ticks);
        }

        [TestMethod]
        public void TestKeyHashCode()
        {
            var dictionary = new Dictionary<PublicKey, PrivateKeyNotWallet>();
            foreach (var key in keys)
            {
                var account = PrivateKeyNotWallet.FromBase64(key);
                Assert.IsFalse(dictionary.ContainsKey(account));
                dictionary.Add(account, account);
                Assert.IsTrue(dictionary.ContainsKey(account));

                var copy = PrivateKeyNotWallet.FromBase64(key);
                Assert.IsTrue(dictionary.ContainsKey(copy));
            }
        }

        [TestMethod]
        public void TestAddressEquality()
        {
            var address1 = new Address("qy8x6fln8aurt2xf29f3vn2gjydxnvcm9f50aqwe");
            var address2 = new Address("qy8x6fln8aurt2xf29f3vn2gjydxnvcm9f50aqwe");

            Assert.AreEqual(address1, address2);
            Assert.AreEqual(address1.GetHashCode(), address2.GetHashCode());
        }
    }
}
