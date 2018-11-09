using System;
using System.Diagnostics;
using Caasiope.Node.Managers;
using Caasiope.Node.Processors;
using Caasiope.Node.Processors.Commands;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Services;
using Helios.Common.Synchronization;

namespace Caasiope.Node.Services
{
    public interface ILedgerService : IService
    {
        LedgerManager LedgerManager { get; }
        SynchronizedBlockingState<LedgerStatus> State { get; }

        void SetNextLedger(SignedLedger signed, Action onFinish);
    }

    public enum LedgerStatus
    {
        Initializing,
        Updating,
        Updated,
    }

    // this service only updates the current ledger state
    public class LedgerService : ThreadedService, ILedgerService
    {
        public LedgerManager LedgerManager { get; }

        private readonly LedgerCommandProcessor commands;

        public SynchronizedBlockingState<LedgerStatus> State { get; } = new SynchronizedBlockingState<LedgerStatus>();
        
        public LedgerService(Network network)
        {
            LedgerManager = new LedgerManager(network, Logger);
            commands = new LedgerCommandProcessor();
        }

        protected override void OnInitialize()
        {
            DatabaseService.InitializedHandle.WaitOne();
            DataTransformationService.InitializedHandle.WaitOne();
        }

        protected override void OnStart()
        {
            LiveService.StartedHandle.WaitOne();

            var needSetInitial = InitializeLedgerManager();

            State.SetAndWait(LedgerStatus.Updated); // TODO initialize

            if (needSetInitial)
                LedgerService.LedgerManager.SetNextLedger(LedgerManager.GetSignedLedger(), null);
        }

        private bool InitializeLedgerManager()
        {
            var last = GetLastLedger();
            var needSetInitial = false;
            if (last == null)
            {
                last = LoadInitialLedger();
                needSetInitial = true;
            }

            LedgerManager.Initialize(last, needSetInitial);

            if(!needSetInitial)
                CheckMerkleRoot();
            return needSetInitial;
        }

        private SignedLedger GetLastLedger()
        {
            return DatabaseService.ReadDatabaseManager.GetLastLedgerFromRaw();
        }

        private SignedLedger LoadInitialLedger()
        {
            return InitialLedgerConfiguration.LoadLedger(InitialLedgerConfiguration.GetDefaultPath());
        }

        private void CheckMerkleRoot()
        {
            var merkle = LedgerService.LedgerManager.GetMerkleRoot();
            var ledger = LedgerService.LedgerManager.GetSignedLedger();

            // TODO why debug assert ?
            Debug.Assert(ledger.Ledger.MerkleHash.Equals(merkle.Hash));
        }

        protected override void OnStop()
        {
        }

        protected override void Run()
        {
            while (commands.TryProcessOne()) { }
        }

        public void SetNextLedger(SignedLedger signed, Action onFinish)
        {
            // queue command
            AddCommand(new SetNextLedgerCommand(signed, onFinish));
        }

        private void AddCommand<T>(LedgerCommand<T> liveCommand)
        {
            commands.Add(liveCommand);
            trigger.Set();
        }
    }
}