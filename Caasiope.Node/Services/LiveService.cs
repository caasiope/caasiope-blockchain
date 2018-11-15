using System.Collections.Generic;
using Caasiope.JSON.Helpers;
using Caasiope.Node.Connections;
using Caasiope.Node.Managers;
using Caasiope.Node.Processors;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Services;
using Helios.Common.Synchronization;

namespace Caasiope.Node.Services
{
    public interface ILiveService : IService
    {
        AccountManager AccountManager { get; }
        IssuerManager IssuerManager { get; }
        PersistenceManager PersistenceManager { get; }
        SignedTransactionManager SignedTransactionManager { get; }
        TransactionManager TransactionManager { get; }
        SignatureManager SignatureManager { get; }
        ValidatorManager ValidatorManager { get; }
        CatchupManager CatchupManager { get; }

        LiveCommand<T> AddCommand<T>(LiveCommand<T> liveCommand);
    }

    // rename to something live current state
    public class LiveService : ThreadedService, ILiveService
    {
        private readonly LiveCommandProcessor commands = new LiveCommandProcessor();

        // Managers
        public AccountManager AccountManager { get; } = new AccountManager();
        public IssuerManager IssuerManager { get; } = new IssuerManager();
        public PersistenceManager PersistenceManager { get; } = new PersistenceManager();
        public SignedTransactionManager SignedTransactionManager { get; } = new SignedTransactionManager();
        public TransactionManager TransactionManager { get; } = new TransactionManager();
        public SignatureManager SignatureManager { get; } = new SignatureManager();
        public ValidatorManager ValidatorManager { get; } = new ValidatorManager();
        public CatchupManager CatchupManager { get; } = new CatchupManager();

        private SynchronizedBlockingState<LedgerStatus>.Listener state;

        private readonly int quorum;
        private readonly List<PublicKey> validators;
        private readonly List<Issuer> issuers;

        public LiveService(int quorum, List<PublicKey> validators, List<Issuer> issuers)
        {
            this.quorum = quorum;
            this.validators = validators;
            this.issuers = issuers;
        }

        protected override void OnInitialize()
        {
            DatabaseService.InitializedHandle.WaitOne();

            state = LedgerService.State.CreateListener();
            RegisterWaitHandle(state.CancelEvent, HandleLedgerStateChanged, true);
        }

        private void HandleLedgerStateChanged()
        {
            state.NextState();
        }

        // TODO move
        private void Validate()
        {
            var max = LedgerService.LedgerManager.GetLedgerLight().Height;
            for (long height = 0; height < max; height++)
            {
                Logger.Log($"Validating Ledger {height}");
                var signed = DatabaseService.ReadDatabaseManager.GetLedgerFromRaw(height);
                //LedgerService.LedgerManager.Finalize(signed);
            }
        }

        protected override void OnStart()
        {
            DataTransformationService.StartedHandle.WaitOne();
            DataTransformationService.WaitTransformationCompleted();

            var accounts = DatabaseService.ReadDatabaseManager.GetAccounts();

            // load all the accounts
            AccountManager.Initialize(accounts);
            IssuerManager.Initialize(issuers);
            ValidatorManager.Initialize(validators, quorum);
            TransactionManager.Initialize();
            PersistenceManager.Initialize();
            CatchupManager.Initialize(Logger);
            SignedTransactionManager.Initialize();

            TransactionManager.TransactionReceived += TransactionManager.SendTransactionReceivedNotification;
            ConnectionService.OnSessionConnected(SendSignedNewLedgerNotification);
        }


        protected override void OnStop()
        {
        }

        protected override void Run()
        {
            if (state.State != LedgerStatus.Updated)
                return;

            while (commands.TryProcessOne())
            {
            }
        }

        public LiveCommand<T> AddCommand<T>(LiveCommand<T> liveCommand)
        {
            commands.Add(liveCommand);
            trigger.Set();
            return liveCommand;
        }

        // TODO why here ?
        private void SendSignedNewLedgerNotification(IConnectionSession session)
        {
            var signedLedger = LedgerService.LedgerManager.GetSignedLedger();
            var notification = NotificationHelper.CreateSignedNewLedgerNotification(signedLedger);
            ConnectionService.BlockchainChannel.Send(session, notification);
        }
    }
}