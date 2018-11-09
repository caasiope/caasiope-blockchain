using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caasiope.JSON.Helpers;
using Caasiope.Log;
using Caasiope.Node.Sagas;
using Caasiope.Node.Services;
using Caasiope.Node.Trackers;
using Caasiope.Node.Validators;
using Caasiope.Protocol.MerkleTrees;
using Caasiope.Protocol.Types;
using Caasiope.Protocol.Validators;
using Helios.Common.Extensions;
using Helios.Common.Logs;

namespace Caasiope.Node.Managers
{
    public class LedgerManager
    {
		[Injected] public ILiveService LiveService;
		[Injected] public ILedgerService LedgerService;
		[Injected] public IConnectionService ConnectionService;

        public SignedLedgerValidator SignedLedgerValidator;
        private SignedLedger lastLedger;
        public ProtocolVersion Version;
        public readonly Network Network;
        private readonly ILogger logger;
        private readonly ILogger merkleLogger;
        private bool needSetInitialLedger;

        public LedgerManager(Network network, ILogger logger)
        {
            Network = network;
            this.logger = logger;
            merkleLogger = new LoggerAdapter("StartupMerkleLogger");
        }

        public void Initialize(SignedLedger lastLedger, bool needToSetInitial)
        {
            Injector.Inject(this);
            needSetInitialLedger = needToSetInitial;
            var validatorManager = LiveService.ValidatorManager;
            SignedLedgerValidator = new SignedLedgerValidator(validatorManager.GetValidators(), validatorManager.Quorum, Network);

            InitializeLedger(lastLedger);
        }

        private void InitializeLedger(SignedLedger lastLedger)
        {
            this.lastLedger = lastLedger;
            Debug.Assert(SignedLedgerValidator.Validate(this.lastLedger) == LedgerValidationStatus.Ok, "Last Ledger is not valid"); // Most likely not enough signatures (see quorum)
            Version = GetLedgerLight().Version;
        }

        public LedgerMerkleRoot GetMerkleRoot()
        {
            return new LedgerMerkleRoot(LiveService.AccountManager.GetAccounts(), GetDeclarations(), merkleLogger);
        }

        private IEnumerable<TxDeclaration> GetDeclarations()
        {
            var result = new List<TxDeclaration>();
            result.AddRange(LiveService.MultiSignatureManager.GetMultiSignatures());
            result.AddRange(LiveService.HashLockManager.GetHashLocks());
            result.AddRange(LiveService.TimeLockManager.GetTimeLocks());
            return result;
        }

        // call from command
        public void SetNextLedger(SignedLedger signed, Action onFinish)
        {
            if (!ValidateSignedLedgerInternal(signed))
                return;

            if (!ValidateSignatures(signed))
                return;

            try
            {
                // wait for other thread
                LedgerService.State.SetAndWait(LedgerStatus.Updating); // TODO really call from here?
                SetNextLedger(signed);
                onFinish.Call();
            }
            finally
            {
                // wait for other thread
                LedgerService.State.SetAndWait(LedgerStatus.Updated); // TODO really call from here?
            }
        }

        private void SetNextLedger(SignedLedger signed)
        {
            Debug.Assert(ValidateSignedLedgerInternal(signed));
            Debug.Assert(ValidateSignatures(signed));

            // save all    
            Finalize(signed);

            needSetInitialLedger = false;
            logger.Log($"Ledger Finalized. Height : {signed.Ledger.LedgerLight.Height} Transactions : {signed.Ledger.Block.Transactions.Count()} ");
        }

        private bool ValidateSignedLedgerInternal(SignedLedger signed)
        {
            if (needSetInitialLedger)
                return true;
            
            return ValidateSignedLedger(signed);
        }

        public bool ValidateSignedLedger(SignedLedger signed)
        {
            if (signed.Ledger.LedgerLight.Height != GetNextHeight())
                return false;

            if (!signed.Ledger.LedgerLight.Lastledger.Equals(GetLastLedgerHash()))
                return false;

            return true;
        }

        public bool ValidateSignatures(SignedLedger signedLedger)
        {
            // TODO check that fee transaction = total fees

            // TODO is this correct?
            var status = SignedLedgerValidator.Validate(signedLedger);
            if (status == LedgerValidationStatus.Ok)
                return true;

            logger.Log($"LedgerManager : SignedLedger signature validation Failed! {status}");

            return false;
        }

        //private bool ValidateFeeZeroSum(SignedLedger signedLedger)
        //{
        //    var inputs = new List<TxInput>();
        //    foreach (var signed in signedLedger.Ledger.Block.Transactions)
        //    {
        //        if (signed.Transaction.Fees != null)
        //            inputs.Add(signed.Transaction.Fees);
        //    }

        //    var outputs = ComputeFeeOutputs(signedLedger.Ledger.Block.Transactions, accountManager.GetFeeAccount().Address);

        //    foreach (var txOutput in outputs)
        //    {

        //    }
        //    return dictionary.Values.All(a => a == 0);
        //}


        public LedgerHash GetLastLedgerHash()
        {
            return lastLedger.Hash;
        }

        public long GetNextHeight()
        {
            return GetLedgerLight().Height + 1;
        }

        // TODO wait for other threads to release ressources, maybe via live service
        private void Finalize(SignedLedger signedLedger)
        {
            using (var bard = new FinalizeLedgerBard(new FinalizeLedgerFolklore(signedLedger), Contextualize(new FinalizeLedgerSaga())))
            {
                byte index = 0;
                foreach (var signed in signedLedger.Ledger.Block.Transactions)
                {
                    // TODO make a better validation
                    // Debug.Assert(signed.Transaction.Expire > signedLedger.Ledger.LedgerLight.BeginTime);
                    Debug.Assert(signedLedger.Ledger.Block.FeeTransactionIndex == index++ || LiveService.TransactionManager.TransactionValidator.ValidateBalance(bard.Saga, signed.Transaction.GetInputs()));
                    LiveService.SignedTransactionManager.Execute(bard.Saga, signed.Transaction);
                }
            }

            lastLedger = signedLedger;

            BroadcastNewLedger(lastLedger);
        }

        private void BroadcastNewLedger(SignedLedger signedLedger)
        {
            var message = NotificationHelper.CreateSignedNewLedgerNotification(signedLedger);
            // broadcast the hash of the new ledger with the signature.
            ConnectionService.BlockchainChannel.Broadcast(message);
            logger.Log("Broadcast Signed New Ledger");
        }

        private FinalizeLedgerSaga Contextualize(FinalizeLedgerSaga finalizeLedgerSaga)
        {
            // finalizeLedgerSaga.SetServices(services);
            return finalizeLedgerSaga;
        }

        public LedgerLight GetLedgerLight()
        {
            return lastLedger.Ledger.LedgerLight;
        }

        public SignedLedger GetSignedLedger()
        {
            return lastLedger;
        }

        public long GetLedgerBeginTime()
        {
            return GetLedgerLight().Timestamp; // TODO beginTime or beginTime + 1 ?
        }
    }
}