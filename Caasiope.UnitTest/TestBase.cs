using System;
using System.Collections.Generic;
using System.Threading;
using Caasiope.Database.Managers;
using Helios.Common;
using Helios.JSON;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Caasiope.NBitcoin;
using Caasiope.JSON.Helpers;
using Caasiope.Node;
using Caasiope.Node.Services;
using Caasiope.Protocol;
using Caasiope.Protocol.Types;
using Caasiope.Protocol.Validators;
using Caasiope.Protocol.Validators.Transactions;
using Caasiope.UnitTest.FakeServices;
using ResultCode = Caasiope.Node.ResultCode;

namespace Caasiope.UnitTest
{
    public abstract class TestBase
    {
	    protected static Network Network;

        protected readonly PrivateKeyNotWallet BTC_ISSUER = PrivateKeyNotWallet.FromBase64("n6uPVvs4x8A80yt3yw/KTUSEZQpqsu0FM/gSm7EPmXs=");

        protected static string[] keys =
        {
            "7r7oFxKhhaH7UvMLpUXlcIEk0WWx7i4nw6BVnrKCmLk=",
            "QkQ07ERvruv0idJ6e0xDX2GbpYcuLB+dPueldNyd5xA=",
            "cyg1lEqj83Xa00KHdtab5tCyvhKVC6q84YkEFnERbPI=",
            "WgqyI72GAn/up/H6uSajmxh3njrbXjDxoxXA56x69JQ=",
            "yNV1hPckx4F/uyh9Y6OwbzlYMDVU/DhJlUs7zkr44DE="
        };


        static TestBase()
        {
			AssertionHandler.CatchAssertions();
            NodeConfiguration.Initialize(LocalNetwork.Instance.Name);
            Network = NodeConfiguration.GetNetwork();
        }

        protected class TestContext : IDisposable
        {
            public IDatabaseService DatabaseService;
			public IConnectionService ConnectionService;
			public ILiveService LiveService;
			public ILedgerService LedgerService;
			public IDataTransformationService DataTransformationService;

            public readonly ServiceManager services = new ServiceManager();
            private int height = 1;
            private readonly bool isSave;
            private readonly DummyLedgerCreator dummyLedgerCreator;

            public TestContext(bool isSave)
            {
                if (Thread.CurrentThread.Name == null)
                    Thread.CurrentThread.Name = "UNIT TEST";

                this.isSave = isSave;
                var factory = new FakeNodeServiceFactory();
                ConnectionService = services.Add(factory.CreateConnectionService());
                DatabaseService = services.Add(factory.CreateDatabaseService());
                LiveService = services.Add(factory.CreateLiveService());
                LedgerService = services.Add(factory.CreateLedgerService());
                DataTransformationService = services.Add(factory.CreateDataTransformationService());

                if (this.isSave) WipeDatabase();

                DatabaseService.IsSave = isSave;
                services.Initialize();
                services.Start();

                dummyLedgerCreator = new DummyLedgerCreator(LedgerService, LiveService);
            }

            private void WipeDatabase()
            {
                WipeDatabaseHelper.WipeDatabase();
            }

            public void Dispose()
            {
                if(isSave)
                    DatabaseService.WaitSaveCompleted();

                Injector.Clear();
                // ServiceManager.Stop();
            }

            public void SendRequest(RequestMessage request, ResultCode result = ResultCode.Success)
            {
                var actual = ResultCode.Failed;
                var evt = new AutoResetEvent(false);
                Assert.IsTrue(ConnectionService.BlockchainChannel.Dispatcher.Dispatch(new FakeP2PConnection().FakeSession(), request,
                    (response, code) =>
                    {
                        actual = code;
                        evt.Set();
                    }));
                evt.WaitOne();
                Assert.AreEqual((byte)result, (byte)actual);
            }

            public void SendTransaction(SignedTransaction signed, ResultCode expected = ResultCode.Success)
            {
                SendRequest(RequestHelper.CreateSendSignedTransactionRequest(signed), expected);
            }

            public bool TryCreateNextLedger()
            {
                return dummyLedgerCreator.TryCreateNextLedger();
            }

            public bool TryGetAccount(string encoded, out Account account)
            {
                return LedgerService.LedgerManager.LedgerState.TryGetAccount(new Address(encoded), out account);
            }
        }

        protected TestContext CreateContext(bool isSave = false)
        {
            return new TestContext(isSave);
        }
        
        private int count;
        public PrivateKeyNotWallet CreateAccount()
        {
            return PrivateKeyNotWallet.FromBase64(keys[count++]);
        }

        protected SignedTransaction Transfer(PrivateKeyNotWallet sender, Address receiver, Currency currency, Amount amount, PrivateKeyNotWallet feePayer = null, long? fees = null, List<TxDeclaration> declarations = null, TransactionMessage message = null)
        {
            return Transfer(sender, new List<PrivateKeyNotWallet> {sender}, receiver, currency, amount, declarations, feePayer, fees, message);
        }

        protected SignedTransaction Transfer(Address sender, List<PrivateKeyNotWallet> signers, Address receiver, Currency currency, Amount amount, List<TxDeclaration> declarations = null, PrivateKeyNotWallet feePayer = null, long? fees = null, TransactionMessage message = null)
        {
            if (message == null)
                message = TransactionMessage.Empty;
            var inputs = new List<TxInput>()
            {
                new TxInput(sender, currency, amount),
            };

            var outputs = new List<TxOutput>()
            {
                new TxOutput(receiver, currency, amount),
            };

            var feeTxInput = fees == null ? null : new TxInput(feePayer, Currency.BTC, fees);

            var transaction = new Transaction(declarations ?? new List<TxDeclaration>(), inputs, outputs, message, DateTime.UtcNow.AddMinutes(1).ToUnixTimestamp(), feeTxInput);

            var signed = new SignedTransaction(transaction);
            foreach (var signer in signers)
                Assert.IsTrue(signer.SignTransaction(signed, Network));

            if (feePayer != null && feePayer.Address != sender)
                Assert.IsTrue(feePayer.SignTransaction(signed, Network));

            return signed;
        }

        protected bool CheckSignatures(SignedTransaction signed, long timestamp = 0)
        {
            if (timestamp == 0)
                timestamp = DateTime.UtcNow.ToUnixTimestamp();
            return signed.CheckSignatures(TransactionRequiredValidationFactory.Instance, Network, timestamp);
        }

        private class TransactionRequiredValidationFactory : Caasiope.Protocol.Validators.TransactionRequiredValidationFactory
        {
            public static readonly TransactionRequiredValidationFactory Instance = new TransactionRequiredValidationFactory();

            public override bool TryGetRequiredValidations(LedgerState state, Address address, List<TxDeclaration> declarations, out TransactionRequiredValidation required)
            {
                switch (address.Type)
                {
                    case AddressType.ECDSA:
                        required = new AddressRequiredSignature(address);
                        return true;
                }
                required = null;
                return false;
            }
        }

        private class DummyLedgerCreator
        {
            private readonly PrivateKeyNotWallet validator = PrivateKeyNotWallet.FromBase64("AKiWI3xivi2tsMz1Sh/v+0WrJaM60t/3h/qcEfu6r1pH");
            private readonly List<SignedTransaction> pendingTransactions = new List<SignedTransaction>();
            private readonly ILedgerService ledgerService;

            public DummyLedgerCreator(ILedgerService ledgerService, ILiveService liveService)
            {
                liveService.TransactionManager.TransactionReceived += tx => pendingTransactions.Add(tx);
                this.ledgerService = ledgerService;
            }

            private SignedLedger GetLedger()
            {
                var light = new LedgerLight(
                    ledgerService.LedgerManager.GetNextHeight(),
                    DateTime.Now.ToUnixTimestamp() + 1, // TODO ? ledgerService.LedgerManager.GetLedgerBeginTime() + 10,
                    ledgerService.LedgerManager.GetLastLedgerHash(),
                    ledgerService.LedgerManager.GetLedgerLight().Version);

                var block = Block.CreateBlock(light.Height, pendingTransactions);

                var ledger = new Ledger(light, block, ledgerService.LedgerManager.GetMerkleRootHash());
                var signed = new SignedLedger(ledger);

                validator.SignLedger(signed, NodeConfiguration.GetNetwork());

                return signed;
            }

            public bool TryCreateNextLedger()
            {
                var evt = new AutoResetEvent(false);
                ledgerService.SetNextLedger(GetLedger(), () => evt.Set());
                if (!evt.WaitOne(3000))
                    return false;

                pendingTransactions.Clear();
                return true;
            }
        }
    }
}