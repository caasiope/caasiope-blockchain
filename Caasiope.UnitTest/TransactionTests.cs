using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Caasiope.JSON.Helpers;
using Caasiope.NBitcoin;
using Caasiope.Node;
using Caasiope.Node.Managers;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Caasiope.UnitTest
{
    [TestClass]
    public class TransactionTests : TestBase
    {
        [TestMethod]
        public void TestInitializeDatabase()
        {
            // we load the initial block in database
            using (var context = CreateContext(true))
            {
                context.DataTransformationService.WaitTransformationCompleted();

                var processedHeights = context.DatabaseService.ReadDatabaseManager.GetHeightTables().ToDictionary(_ => _.TableName);
                var expectedHeights = context.DataTransformationService.DataTransformerManager.GetInitialTableHeights();

                foreach (var expected in expectedHeights)
                {
                    Debug.Assert(processedHeights[expected.TableName].Height == 0);
                }

                var readedLedger = context.DatabaseService.ReadDatabaseManager.GetLastLedger();
                var ledger = context.LedgerService.LedgerManager.GetSignedLedger();

                Debug.Assert(ledger.Ledger.LedgerLight.Height == 0);
                Debug.Assert(ledger.Hash.Equals(readedLedger.Hash));
            }
        }

        [TestMethod]
        public void TestTransactionValidation_Transfer()
        {
            using (var context = CreateContext())
            {
                var sender = CreateAccount();
                var receiver = CreateAccount();

                // transaction cannot have duplicated signature
                var transaction = Transfer(sender, receiver, Currency.BTC, 10);
                transaction.Signatures.Add(transaction.Signatures.First());
                context.SendTransaction(transaction, ResultCode.CannotReadSignedTransaction);

                // transaction doesn't have required signature
                transaction = Transfer(sender, receiver, Currency.BTC, 10);
                transaction.Signatures.Clear();
                context.SendTransaction(transaction, ResultCode.TransactionValidationFailed);

                // wrong signature
                receiver.SignTransaction(transaction, Network);
                context.SendTransaction(transaction, ResultCode.TransactionValidationFailed);

                // transaction have no inputs but have an output
                transaction = Transfer(sender, receiver, Currency.BTC, 10);
                transaction.Transaction.Inputs.Clear();
                context.SendTransaction(transaction, ResultCode.TransactionValidationFailed);

                // transaction have no outputs but have an input
                transaction = Transfer(sender, receiver, Currency.BTC, 10);
                transaction.Transaction.Outputs.Clear();
                context.SendTransaction(transaction, ResultCode.TransactionValidationFailed);

                // transaction have no inputs and outputs 
                transaction = Transfer(sender, receiver, Currency.BTC, 10);
                transaction.Transaction.Inputs.Clear();
                transaction.Transaction.Outputs.Clear();
                context.SendTransaction(transaction, ResultCode.TransactionValidationFailed);

                // amount in output more than amount in input
                transaction = Transfer(sender, receiver, Currency.BTC, 10);
                var output = transaction.Transaction.Outputs.First();
                var modified = new TxOutput(output.Address, output.Currency, Amount.FromWholeDecimal(99999));
                transaction.Transaction.Outputs.Clear();
                transaction.Transaction.Outputs.Add(modified);
                context.SendTransaction(transaction, ResultCode.TransactionValidationFailed);

                // amount in input is negative
                transaction = Transfer(sender, receiver, Currency.BTC, 10);
                var input = transaction.Transaction.Inputs.First();
                var modifiedInput = new TxInput(input.Address, input.Currency, Amount.FromWholeDecimal(-10));
                transaction.Transaction.Inputs.Clear();
                transaction.Transaction.Inputs.Add(modifiedInput);
                context.SendTransaction(transaction, ResultCode.TransactionValidationFailed);

                // fees are negative
                transaction = Transfer(sender, receiver, Currency.BTC, 10);
                var tx = transaction.Transaction;
                transaction = new SignedTransaction(new Transaction(tx.Declarations, tx.Inputs, tx.Outputs, tx.Message, tx.Expire, new TxInput(sender.Address, Currency.BTC, -1)), transaction.Signatures);
                context.SendTransaction(transaction, ResultCode.TransactionValidationFailed);

            }
            // context.SendTransaction(signedInvalid, ResultCode.CannotReadSignedTransaction);
        }

        [TestMethod]
        public void Transaction_DuplicateDeclarations()
        {
            using (var context = CreateContext())
            {
                var issuer = BTC_ISSUER;

                var signers = new List<Address>() { CreateAccount().Address, CreateAccount().Address };
                var multi = new MultiSignature(signers, 2);
                //  Check multisig
                {
                    var signed = Transfer(issuer, multi.Address, Currency.BTC, 1, null, null, new List<TxDeclaration> { multi, multi });
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }

                // create timelock
                var timelock = new TimeLock(DateTime.Now.AddDays(-1).ToUnixTimestamp());
                // Check timelocks
                {
                    var signed = Transfer(issuer, timelock.Address, Currency.BTC, 1, null, null, new List<TxDeclaration> { timelock, timelock });
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }

                var secret = Secret.GenerateSecret();
                var hashlock = new HashLock(secret.ComputeSecretHash(SecretHashType.SHA256));
                //  Check hashLock
                {
                    var signed = Transfer(issuer, hashlock.Address, Currency.BTC, 1, null, null, new List<TxDeclaration> { hashlock, hashlock });
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }

                var secretRevelation = new SecretRevelation(secret);
                //  Check Secret
                {
                    var signed = Transfer(issuer, hashlock.Address, Currency.BTC, 1, null, null, new List<TxDeclaration> { secretRevelation, secretRevelation });
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }
            }
        }

        [TestMethod]
        public void TestIssueCurrency()
        {
            using (var context = CreateContext())
            {
                var random = CreateAccount();
                var issuer = BTC_ISSUER;

                // random account cant issue
                {
                    var signed = Transfer(random, issuer, Currency.BTC, 10);
                    var isSuccess = ValidateTransaction(context, signed);
                    Assert.IsFalse(isSuccess);
                }

                // issuer account cant issue other currency
                {
                    var signed = Transfer(issuer, random, Currency.LTC, 10);
                    var isSuccess = ValidateTransaction(context, signed);
                    Assert.IsFalse(isSuccess);
                }

                // issuer account can issue
                {
                    var signed = Transfer(issuer, random, Currency.BTC, 10);
                    var isSuccess = ValidateTransaction(context, signed);
                    Assert.IsTrue(isSuccess);
                }
            }
        }

        [TestMethod]
        public void TestPaymentTransactionWithFees_SenderPays()
        {
            using (var context = CreateContext())
            {
                var sender = BTC_ISSUER;
                var receiver = CreateAccount();

                var signed = Transfer(sender, receiver, Currency.BTC, 10, sender, 5);
                context.SendTransaction(signed);

                // create next ledger
                Assert.IsTrue(context.TryCreateNextLedger());

                context.TryGetAccount(receiver.Address.Encoded, out var receiverAccount);
                context.TryGetAccount(sender.Address.Encoded, out var senderAccount);

                Assert.IsTrue(receiverAccount.GetBalance(Currency.BTC) == 10);
                Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == -15);
            }
        }

        [TestMethod]
        public void TestPaymentTransactionWithFees_ReceiverPays()
        {
            using (var context = CreateContext())
            {
                var sender = BTC_ISSUER;
                var receiver = CreateAccount();

                {
                    var signed = Transfer(sender, receiver, Currency.BTC, 5);
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    context.TryGetAccount(receiver.Address.Encoded, out var receiverAccount);
                    context.TryGetAccount(sender.Address.Encoded, out var senderAccount);

                    Assert.IsTrue(receiverAccount.GetBalance(Currency.BTC) == 5);
                    Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == -5);
                }

                {
                    var signed = Transfer(sender, receiver, Currency.BTC, 5, receiver, 5);
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    context.TryGetAccount(receiver.Address.Encoded, out var receiverAccount);
                    context.TryGetAccount(sender.Address.Encoded, out var senderAccount);

                    Assert.IsTrue(receiverAccount.GetBalance(Currency.BTC) == 5);
                    Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == -10);
                }
            }
        }

        [TestMethod]
        public void TestPaymentTransactionWithFees_ReceiverNotEnoughBalance()
        {
            using (var context = CreateContext())
            {
                var issuer = BTC_ISSUER;
                var receiver = CreateAccount();

                var signed1 = Transfer(issuer, receiver, Currency.BTC, 5);
                context.SendTransaction(signed1);
                Assert.IsTrue(context.TryCreateNextLedger());

                var signed2 = Transfer(receiver, issuer, Currency.BTC, 5, receiver, 5);

                var isSuccess = ValidateTransaction(context, signed2);
                Assert.IsFalse(isSuccess);
            }
        }

        private bool ValidateTransaction(TestContext context, SignedTransaction signed)
        {
            // TODO
            var state = context.LedgerService.LedgerManager.LedgerState;
            return context.LiveService.TransactionManager.TransactionValidator.ValidateBalance(state, signed.Transaction.GetInputs());
        }

        [TestMethod]
        public void TestPaymentTransactionWithFees_StrangerPays()
        {
            using (var context = CreateContext())
            {
                var sender = BTC_ISSUER;
                var receiver = CreateAccount();
                var stranger = CreateAccount();

                var signed1 = Transfer(sender, stranger, Currency.BTC, 5);
                context.SendTransaction(signed1);
                Assert.IsTrue(context.TryCreateNextLedger());

                var signed = Transfer(sender, receiver, Currency.BTC, 10, stranger, 5);
                context.SendTransaction(signed);
                Assert.IsTrue(context.TryCreateNextLedger());

                context.TryGetAccount(receiver.Address.Encoded, out var receiverAccount);
                context.TryGetAccount(sender.Address.Encoded, out var senderAccount);
                context.TryGetAccount(stranger.Address.Encoded, out var strangerAccount);

                Assert.IsTrue(receiverAccount.GetBalance(Currency.BTC) == 10);
                Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == -15);
                Assert.IsTrue(strangerAccount.GetBalance(Currency.BTC) == 0);
            }
        }

        [TestMethod]
        public void TestMultiSignature()
        {
            using (var context = CreateContext())
            {
                var receiver = CreateAccount();
                var signer1 = CreateAccount();
                var signer2 = CreateAccount();
                var issuer = BTC_ISSUER;

                var signers = new List<Address>() { signer1.Address, signer2.Address };
                var multi = new MultiSignature(signers, 2);
                var multiAddress = multi.Address;

                // issuer send to multisignature
                {
                    var signed = Transfer(issuer, multiAddress, Currency.BTC, 10, null, null, new List<TxDeclaration>() { multi });
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(multiAddress.Encoded, out var multiAccount));

                    // check that the money has been sent
                    Assert.IsTrue(multiAccount.GetBalance(Currency.BTC) == 10);
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -10);
                }

                // multisignature send to receiver
                {
                    var signed = Transfer(multiAddress, new List<PrivateKeyNotWallet> { signer1, signer2 }, receiver, Currency.BTC, 10);
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(receiver.Address.Encoded, out var receiverAccount));
                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(multiAddress.Encoded, out var multiAccount));

                    // check that the money has been received
                    Assert.IsTrue(multiAccount.GetBalance(Currency.BTC) == 0);
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -10);
                    Assert.IsTrue(receiverAccount.GetBalance(Currency.BTC) == 10);
                }
            }
        }

        [TestMethod]
        public void TestMultiSignature2()
        {
            using (var context = CreateContext())
            {
                var receiver = CreateAccount();
                var signer1 = CreateAccount();
                var signer2 = CreateAccount();
                var issuer = BTC_ISSUER;

                var signers = new List<Address>() { signer1.Address, signer2.Address };
                var multi = new MultiSignature(signers, 2);
                var multiAddress = multi.Address;

                // issuer send to multisignature
                {
                    var signed = Transfer(issuer, multiAddress, Currency.BTC, 3);
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(multiAddress.Encoded, out var multiAccount));

                    // check that the money has been sent
                    Assert.IsTrue(multiAccount.GetBalance(Currency.BTC) == 3);
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -3);
                }

                // multisignature send to receiver without the declaration Fail. Because no required declarations (signatures) found for the multisigaddress
                {
                    var signed = Transfer(multiAddress, new List<PrivateKeyNotWallet> { signer1, signer2 }, receiver, Currency.BTC, 1);
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);

                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(multiAddress.Encoded, out var multiAccount));

                    // check the money 
                    Assert.IsTrue(multiAccount.GetBalance(Currency.BTC) == 3);
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -3);
                }

                // multisignature send to receiver with the declaration PASS
                {
                    var signed = Transfer(multiAddress, new List<PrivateKeyNotWallet> { signer1, signer2 }, receiver, Currency.BTC, 1, new List<TxDeclaration> { multi });
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(multiAddress.Encoded, out var multiAccount));
                    Assert.IsTrue(context.TryGetAccount(receiver.Address.Encoded, out var receiverAccount));

                    // check that the money has been received
                    Assert.IsTrue(multiAccount.GetBalance(Currency.BTC) == 2);
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -3);
                    Assert.IsTrue(receiverAccount.GetBalance(Currency.BTC) == 1);
                }

                // multisignature send to receiver without the declaration PASS because it has already been declared
                {
                    var signed = Transfer(multiAddress, new List<PrivateKeyNotWallet> { signer1, signer2 }, receiver, Currency.BTC, 1);
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(multiAddress.Encoded, out var multiAccount));
                    Assert.IsTrue(context.TryGetAccount(receiver.Address.Encoded, out var receiverAccount));

                    // check that the money has been received
                    Assert.IsTrue(multiAccount.GetBalance(Currency.BTC) == 1);
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -3);
                    Assert.IsTrue(receiverAccount.GetBalance(Currency.BTC) == 2);
                }
            }
        }

        [TestMethod]
        public void TestMultiSignature_NotEnoughSigners()
        {
            using (var context = CreateContext())
            {
                var receiver = CreateAccount();
                var signer1 = CreateAccount();
                var signer2 = CreateAccount();
                var issuer = BTC_ISSUER;

                var signers = new List<Address>() { signer1.Address, signer2.Address };
                var multi = new MultiSignature(signers, 2);
                var multiAddress = multi.Address;

                // issuer send to multisignature
                {
                    var signed = Transfer(issuer, multiAddress, Currency.BTC, 10, null, null, new List<TxDeclaration>() { multi });
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(multiAddress.Encoded, out var multiAccount));

                    // check that the money has been sent
                    Assert.IsTrue(multiAccount.GetBalance(Currency.BTC) == 10);
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -10);
                }

                // multisignature send to receiver
                {
                    var signed = Transfer(multiAddress, new List<PrivateKeyNotWallet> { signer1 }, receiver, Currency.BTC, 10);
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);

                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(multiAddress.Encoded, out var multiAccount));

                    // check that the money has been received
                    Assert.IsTrue(multiAccount.GetBalance(Currency.BTC) == 10);
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -10);
                }
            }
        }

        [TestMethod]
        public void FourThousandTransactionsInBlock()
        {
            Assert.Inconclusive();
            var rnd = new Random();
            using (var context = CreateContext(true))
            {
                var sender = BTC_ISSUER;
                var receiver = CreateAccount();
                var transactions = new List<SignedTransaction>();

                Parallel.For(0, 4000, (i) =>
                {
                    var message = new byte[TransactionMessage.SIZE];
                    rnd.NextBytes(message);
                    transactions.Add(Transfer(sender, receiver, Currency.BTC, 1, sender, null, null, new TransactionMessage(message)));
                });

                foreach (var signed in transactions)
                {
                    context.SendRequest(RequestHelper.CreateSendSignedTransactionRequest(signed));
                }

                // create next ledger
                Assert.IsTrue(context.TryCreateNextLedger());

                context.TryGetAccount(receiver.Address.Encoded, out var receiverAccount);
                context.TryGetAccount(sender.Address.Encoded, out var senderAccount);

                var balance = transactions.Count;
                Assert.IsTrue(receiverAccount.GetBalance(Currency.BTC) == balance);
                Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == -balance);
            }
        }

        [TestMethod]
        public void TestHashLock()
        {
            using (var context = CreateContext())
            {
                var sender = BTC_ISSUER;

                // create secret hash
                var type = SecretHashType.SHA3;
                var secret = Secret.GenerateSecret();
                var hash = secret.ComputeSecretHash(type);
                var hashlock = new HashLock(hash);
                var revelation = new SecretRevelation(secret);

                // send money to hashlock account
                {
                    var signed = Transfer(sender, hashlock.Address, Currency.BTC, 10);
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(sender.Address.Encoded, out var senderAccount));
                    Assert.IsTrue(context.TryGetAccount(hashlock.Address.Encoded, out var hashlockAccount));

                    // check that the money has been sent
                    Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == -10);
                    Assert.IsTrue(hashlockAccount.GetBalance(Currency.BTC) == 10);
                }

                // try spend money without declaration and revelation
                {
                    var signed = Transfer(hashlock.Address, new List<PrivateKeyNotWallet>(), sender, Currency.BTC, 10);
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }

                // try spend money with declaration but without secret revelation
                {
                    var signed = Transfer(hashlock.Address, new List<PrivateKeyNotWallet>(), sender, Currency.BTC, 10, new List<TxDeclaration>() { hashlock });
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }

                // try spend money with secret revelation but without declaration
                {
                    var signed = Transfer(hashlock.Address, new List<PrivateKeyNotWallet>(), sender, Currency.BTC, 10, new List<TxDeclaration>() { revelation });
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }

                // spend money with hash
                {
                    var signed = Transfer(hashlock.Address, new List<PrivateKeyNotWallet>(), sender, Currency.BTC, 10, new List<TxDeclaration>() { hashlock, revelation });
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(sender.Address.Encoded, out var senderAccount));
                    Assert.IsTrue(context.TryGetAccount(hashlock.Address.Encoded, out var hashlockAccount));

                    // check that the money has been sent
                    Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == 0);
                    Assert.IsTrue(hashlockAccount.GetBalance(Currency.BTC) == 0);
                }
            }
        }

        [TestMethod]
        public void TestTimeLock()
        {
            using (var context = CreateContext())
            {
                var sender = BTC_ISSUER;

                // We have an seed block, which has been generated some time ago, so for the first timelock to be unlocked we get the time from it
                var blockBegin = Utils.UnixTimeToDateTime(context.LedgerService.LedgerManager.GetLedgerBeginTime()).DateTime;
                // create timelock unlocked
                var timeunlocked = new TimeLock(blockBegin.AddDays(-1).ToUnixTimestamp());

                // send money to unlocked account
                {
                    var signed = Transfer(sender, timeunlocked.Address, Currency.BTC, 10);
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(sender.Address.Encoded, out var senderAccount));
                    Assert.IsTrue(context.TryGetAccount(timeunlocked.Address.Encoded, out var timeunlockedAccount));

                    // check that the money has been sent
                    Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == -10);
                    Assert.IsTrue(timeunlockedAccount.GetBalance(Currency.BTC) == 10);
                }

                // try spend money without declaration
                {
                    var signed = Transfer(timeunlocked.Address, new List<PrivateKeyNotWallet>(), sender, Currency.BTC, 10);
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }

                // spend money with declaration
                {
                    var signed = Transfer(timeunlocked.Address, new List<PrivateKeyNotWallet>(), sender, Currency.BTC, 10, new List<TxDeclaration>() { timeunlocked });
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(sender.Address.Encoded, out var senderAccount));
                    Assert.IsTrue(context.TryGetAccount(timeunlocked.Address.Encoded, out var timeunlockedAccount));

                    // check that the money has been sent
                    Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == 0);
                    Assert.IsTrue(timeunlockedAccount.GetBalance(Currency.BTC) == 0);
                }

                // create timelock locked
                var timelocked = new TimeLock(DateTime.Now.AddDays(1).ToUnixTimestamp());
                // send money to locked account
                {
                    var signed = Transfer(sender, timelocked.Address, Currency.BTC, 10);
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(sender.Address.Encoded, out var senderAccount));
                    Assert.IsTrue(context.TryGetAccount(timelocked.Address.Encoded, out var timelockedAccount));

                    // check that the money has been sent
                    Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == -10);
                    Assert.IsTrue(timelockedAccount.GetBalance(Currency.BTC) == 10);
                }

                // try spend money when locked
                {
                    var signed = Transfer(timelocked.Address, new List<PrivateKeyNotWallet>(), sender, Currency.BTC, 10, new List<TxDeclaration>() { timelocked });
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }
            }
        }

        [TestMethod]
        public void TestUnspendableTimeLock()
        {
            using (var context = CreateContext())
            {
                var sender = BTC_ISSUER;
                // create unspendable timelock unlocked
                var timeLock = new TimeLock(0);
                {
                    var signed = Transfer(sender, timeLock.Address, Currency.BTC, 10);
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(sender.Address.Encoded, out var senderAccount));
                    Assert.IsTrue(context.TryGetAccount(timeLock.Address.Encoded, out var timelockAccount));

                    // check that the money has been sent
                    Assert.IsTrue(senderAccount.GetBalance(Currency.BTC) == -10);
                    Assert.IsTrue(timelockAccount.GetBalance(Currency.BTC) == 10);
                }

                // try spend money when locked
                {
                    var signed = Transfer(timeLock.Address, new List<PrivateKeyNotWallet>(), sender, Currency.BTC, 10, new List<TxDeclaration> { timeLock });
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }
            }
        }

        [TestMethod]
        public void TestHashTimeLock()
        {
            // TODO CANCEL sender + receiver

            // we can emulate hashtime lock behaviour
            using (var context = CreateContext())
            {
                var issuer = BTC_ISSUER;

                var sender = CreateAccount();
                var receiver = CreateAccount();

                var timeunlocked = new TimeLock(DateTime.Now.AddDays(-1).ToUnixTimestamp());
                var timelocked = new TimeLock(DateTime.Now.AddDays(1).ToUnixTimestamp());

                // TODO we are missing Hash160 : SHA256 + ripemd160
                // create secret hash
                var type = SecretHashType.SHA3;
                var secret = Secret.GenerateSecret();
                var hash = secret.ComputeSecretHash(type);
                var hashlock = new HashLock(hash);
                var revelation = new SecretRevelation(secret);

                // CLAIM receiver + hashlock
                var claim = new MultiSignature(new List<Address>() { receiver.Address, hashlock.Address }, 2);
                // TIMEOUT sender + timelock
                var timeoutlocked = new MultiSignature(new List<Address>() { sender.Address, timelocked.Address }, 2);
                var timeoutunlocked = new MultiSignature(new List<Address>() { sender.Address, timeunlocked.Address }, 2);

                var hashtimelocked = new MultiSignature(new List<Address>() { claim.Address, timeoutlocked.Address }, 1);
                var hashtimeunlocked = new MultiSignature(new List<Address>() { claim.Address, timeoutunlocked.Address }, 1);

                // send money to hashtimelocked account
                {
                    var signed = Transfer(issuer, hashtimelocked.Address, Currency.BTC, 10, null, null, new List<TxDeclaration>() { hashtimelocked });
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(hashtimelocked.Address.Encoded, out var hashtimelockedAccount));

                    // check that the money has been sent
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -10);
                    Assert.IsTrue(hashtimelockedAccount.GetBalance(Currency.BTC) == 10);
                }

                // try timeout from hashtimelocked
                {
                    var signed = Transfer(hashtimelocked.Address, new List<PrivateKeyNotWallet>() { sender }, sender, Currency.BTC, 10, new List<TxDeclaration>() { timelocked, timeoutlocked });
                    context.SendTransaction(signed, ResultCode.TransactionValidationFailed);
                }

                // claim from hashtimelocked
                {
                    var signed = Transfer(hashtimelocked.Address, new List<PrivateKeyNotWallet>() { receiver }, issuer.Address, Currency.BTC, 10, new List<TxDeclaration>() { hashlock, claim, revelation });
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    // update data
                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(hashtimelocked.Address.Encoded, out var hashtimelockedAccount));

                    // check that the money has been sent
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == 0);
                    Assert.IsTrue(hashtimelockedAccount.GetBalance(Currency.BTC) == 0);
                }

                // send money to hashtimeunlocked account
                {
                    var signed = Transfer(issuer, hashtimeunlocked.Address, Currency.BTC, 10, null, null, new List<TxDeclaration>() { hashtimeunlocked });
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(hashtimeunlocked.Address.Encoded, out var hashtimeunlockedAccount));

                    // check that the money has been sent
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == -10);
                    Assert.IsTrue(hashtimeunlockedAccount.GetBalance(Currency.BTC) == 10);
                }

                // timeout from hashtimeunlocked
                {
                    var signed = Transfer(hashtimeunlocked.Address, new List<PrivateKeyNotWallet>() { sender }, issuer.Address, Currency.BTC, 10, new List<TxDeclaration>() { timeunlocked, timeoutunlocked });
                    context.SendTransaction(signed);
                    Assert.IsTrue(context.TryCreateNextLedger());

                    // update data
                    Assert.IsTrue(context.TryGetAccount(issuer.Address.Encoded, out var issuerAccount));
                    Assert.IsTrue(context.TryGetAccount(hashtimeunlocked.Address.Encoded, out var hashtimeunlockedAccount));

                    // check that the money has been sent
                    Assert.IsTrue(issuerAccount.GetBalance(Currency.BTC) == 0);
                    Assert.IsTrue(hashtimeunlockedAccount.GetBalance(Currency.BTC) == 0);
                }
            }
        }
    }
}