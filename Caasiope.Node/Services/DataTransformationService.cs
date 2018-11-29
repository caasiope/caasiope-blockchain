using System;
using System.Collections.Concurrent;
using Caasiope.Node.Managers;
using Caasiope.Protocol.Types;
using Helios.Common.Concepts.Services;
using Helios.Common.Extensions;

namespace Caasiope.Node.Services
{
    public interface IDataTransformationService : IService
    {
        void Transform(SignedLedgerState ledger);
        void WaitTransformationCompleted();
        void OnTransform(Action<SignedLedgerState> callback);
        DataTransformerManager DataTransformerManager { get; }
    }

    public class DataTransformationService : ThreadedService, IDataTransformationService
    {
        public DataTransformerManager DataTransformerManager { get; } = new DataTransformerManager(); 
        private readonly LedgerTransformationManager ledgerTransformationManager = new LedgerTransformationManager();
        private readonly ConcurrentQueue<SignedLedgerState> queue = new ConcurrentQueue<SignedLedgerState>();
        public Action<SignedLedgerState> onTransform;

        protected override void OnInitialize()
        {
            DatabaseService.InitializedHandle.WaitOne();

            DataTransformerManager.Initialize();
            ledgerTransformationManager.Initialize(Logger);
        }

        protected override void OnStart()
        {
            DataTransformerManager.Start();
            ledgerTransformationManager.Start();
            // we run at startup to be sure that SQL db is in a good state
            trigger.Set();
        }

        protected override void OnStop()
        {
            DataTransformerManager.Stop();
        }

        protected override void Run()
        {
            if (queue.TryDequeue(out var ledger))
            {
                ledgerTransformationManager.ProcessLedger(ledger);
                trigger.Set();
            }
        }

        public void Transform(SignedLedgerState ledger)
        {
            onTransform.Call(ledger);
            queue.Enqueue(ledger);
            trigger.Set();
        }

        public void OnTransform(Action<SignedLedgerState> callback)
        {
            onTransform += callback;
        }

        // TODO May be should be more complex
        // wait for all pending transformations to be completed
        public void WaitTransformationCompleted()
        {
            if (!IsRunning)
                throw new NotImplementedException();
            // return; // how can we go here ?

            ledgerTransformationManager.WaitFinished();
        }
    }
}