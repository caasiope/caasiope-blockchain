using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.JSON.Helpers;
using Caasiope.NBitcoin;
using Caasiope.Protocol.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Caasiope.UnitTest
{
    [TestClass]
    public class LedgerTests : TestBase
    {
        [TestMethod]
        public void TestSetNextLedger()
        {
            // Create ledger with multisig, timelock, hashlock, transfer transactions
            // set ledger 
            // verify everything is ok
            Assert.Fail();
        }

        [TestMethod]
        public void TestCreateLedger()
        {
            using (var context = CreateContext())
            {
                var sender = BTC_ISSUER;

                var receiver = CreateAccount();

                var transaction = Transfer(sender, receiver, Currency.BTC, 10);

                // send transaction
                context.SendRequest(RequestHelper.CreateSendSignedTransactionRequest(transaction));

                // create next ledger
                Assert.IsTrue(context.TryCreateNextLedger());

                // get account updated
                Assert.IsTrue(context.TryGetAccount(receiver.Address.Encoded, out var account));
                Assert.IsTrue(account.Balances.First(b => b.Currency.Equals(Currency.BTC)).Amount == 10);
            }
        }

        [TestMethod]
        public void LedgerTransformationTestMerkleHash()
        {
            using (var context = CreateContext(true))
            {
                var sender = BTC_ISSUER;
                var signer1 = CreateAccount();
                var signer2 = CreateAccount();

                var signers = new List<Address>() {signer1, signer2};
                var multi = new MultiSignature(signers, 2);

                var secret = Secret.GenerateSecret();
                var hashlock = new HashLock(secret.ComputeSecretHash(SecretHashType.SHA3));

                var timeLock = new TimeLock(DateTime.Now.AddDays(-1).ToUnixTimestamp());

                var signed = Transfer(sender, multi.Address, Currency.BTC, 10, null, null, new List<TxDeclaration>() {multi, hashlock, timeLock});
                context.SendTransaction(signed);

                var signed1 = Transfer(sender, hashlock.Address, Currency.BTC, 10, null, null, new List<TxDeclaration>() {multi, hashlock, timeLock});
                context.SendTransaction(signed1);

                var signed2 = Transfer(sender, timeLock.Address, Currency.BTC, 10, null, null, new List<TxDeclaration>() {multi, hashlock, timeLock});
                context.SendTransaction(signed2);

                // Send address declaration but don't use the address
                var signed3 = Transfer(sender, timeLock.Address, Currency.BTC, 10, null, null, new List<TxDeclaration>() {new MultiSignature(new List<Address>() {CreateAccount(), CreateAccount(), CreateAccount()}, 3)});
                context.SendTransaction(signed3);

                // Send address declaration but don't use the address
                var signed4 = Transfer(sender, timeLock.Address, Currency.BTC, 10, null, null, new List<TxDeclaration>() {new HashLock(Secret.GenerateSecret().ComputeSecretHash(SecretHashType.SHA256))});
                context.SendTransaction(signed4);

                // Send address declaration but don't use the address
                var signed5 = Transfer(sender, timeLock.Address, Currency.BTC, 10, null, null, new List<TxDeclaration>() {new TimeLock(777)});
                context.SendTransaction(signed5);


                Assert.IsTrue(context.TryCreateNextLedger());

                context.DataTransformationService.WaitTransformationCompleted();

                var last = context.LedgerService.LedgerManager.GetMerkleRootHash();

                var fromDb = context.DatabaseService.ReadDatabaseManager.GetLastLedger();

                Assert.IsTrue(last.Equals(fromDb.Ledger.MerkleHash));
            }
        }
    }
}