using System;
using System.Collections.Generic;
using System.Diagnostics;
using Helios.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caasiope.NBitcoin;
using Caasiope.Protocol;
using Caasiope.Protocol.MerkleTrees;
using Caasiope.Protocol.Types;
using Caasiope.Protocol.Compression;
using Caasiope.Protocol.Validators;
using Caasiope.UnitTest.FakeServices;

namespace Caasiope.UnitTest
{
    [TestClass]
    public class ByteStreamTests : TestBase
    {
        [TestMethod]
        public void ZipTest()
        {
            using (CreateContext())
            {
                var signed = CreateSignedLedger();

                byte[] bytes;
                using (var stream = new ByteStream())
                {
                    stream.Write(signed);
                    bytes = stream.GetBytes();
                }

                var unzipped = Zipper.Unzip(Zipper.Zip(bytes));

                Debug.Assert(bytes.IsEqual(unzipped));
            }
        }

        [TestMethod]
        public void SignedTransactionTest()
        {
            using (CreateContext())
            {
                var signed = CreateSignedTransaction();
                var readed = WriteRead<SignedTransaction, ByteStream>(stream => stream.Write(signed), stream => stream.ReadSignedTransaction());

                Assert.IsTrue(signed.CompareSigned(readed));
            }
        }

        [TestMethod]
        public void SignedLedgerTest()
        {
            using (var context = CreateContext())
            {
                var signed = CreateSignedLedger();
                var validator = context.LedgerService.LedgerManager.SignedLedgerValidator;
                Assert.IsTrue(validator.Validate(signed) == LedgerValidationStatus.Ok);

                var readed = WriteRead<SignedLedger, ByteStream>(stream => stream.Write(signed), stream => stream.ReadSignedLedger());

                Assert.IsTrue(validator.Validate(signed) == LedgerValidationStatus.Ok);

                Assert.IsTrue(signed.CompareSigned(readed));
            }
        }

        [TestMethod]
        public void TxDeclarationTest()
        {
            var account1 = CreateAccount();
            var account2 = CreateAccount();

            var secret = Secret.GenerateSecret();
            var declarations = new List<TxDeclaration>()
            {
                new MultiSignature(new []{account1.Account.Address, account2.Account.Address}, 2),
                new HashLock(secret.ComputeSecretHash(SecretHashType.SHA3)),
                new TimeLock(123),
                new SecretRevelation(secret)
            };

            var inputs = new List<TxInput>()
            {
                new TxInput(account1, Currency.BTC, Amount.FromWholeValue(10)),
                new TxInput(account2, Currency.LTC, Amount.FromWholeValue(30)),
            };

            var outputs = new List<TxOutput>()
            {
                new TxOutput(account2, Currency.BTC, Amount.FromWholeValue(10)),
                new TxOutput(account1, Currency.LTC, Amount.FromWholeValue(30)),
            };

            var transaction =  new Transaction(declarations, inputs, outputs, TransactionMessage.Empty, DateTime.UtcNow.Ticks);
            var signed = new SignedTransaction(transaction);

            var readed = WriteRead<SignedTransaction, ByteStream>(stream => stream.Write(signed), stream => stream.ReadSignedTransaction());

            Assert.IsTrue(signed.CompareSigned(readed));
        }

        private SignedLedger CreateSignedLedger()
        {
            var account1 = PrivateKeyNotWallet.FromBase64("AKiWI3xivi2tsMz1Sh/v+0WrJaM60t/3h/qcEfu6r1pH");
            var block = Block.CreateBlock(1, new List<SignedTransaction> { CreateSignedTransaction() });
            var merkle = new LedgerMerkleRoot(new List<Account>(), new List<TxDeclaration>(), new FakeLogger()).Hash;
            var ledger = new Ledger(new LedgerLight(1, DateTime.UtcNow.ToUnixTimestamp(), new LedgerHash(Hash256.Zero.Bytes), new ProtocolVersion(0x1)), block, merkle);
            var signed = new SignedLedger(ledger);
            var hash = signed.Hash;
            signed.AddSignature(account1.CreateSignature(hash, Network));
            return signed;
        }

        private SignedTransaction CreateSignedTransaction()
        {
            var signed = new SignedTransaction(this.CreateExchangeTransaction(out var account1, out var account2));
            var hash = signed.Hash;
            signed.AddSignature(account1.CreateSignature(hash, Network));
            signed.AddSignature(account2.CreateSignature(hash, Network));
            return signed;
        }

        public T WriteRead<T, TStream>(Action<TStream> write, Func<TStream, T> read) where TStream : ByteStream, new()
        {
            byte[] bytes;
            using (var stream = new TStream())
            {
                write(stream);
                bytes = stream.GetBytes();
            }

            using (var stream = (TStream)Activator.CreateInstance(typeof(TStream), bytes))
            {
                return read(stream);
            }
        }
    }
}