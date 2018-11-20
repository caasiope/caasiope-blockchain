using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caasiope.JSON.Helpers;
using Caasiope.Log;
using Caasiope.Node.Services;
using Caasiope.Node.Types;
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
        public ProtocolVersion Version => GetLedgerLight().Version;
        public readonly Network Network;
        private readonly ILogger logger;
        private readonly ILogger merkleLogger;
        private bool needSetInitialLedger;

        public ImmutableLedgerState LedgerState { get; private set; }

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
            var accounts = new Trie<Account>(Address.RAW_SIZE);
            foreach (var account in LiveService.AccountManager.GetAccounts())
                accounts.Add(account.Key.ToRawBytes(), account.Value);

            LedgerState = new ImmutableLedgerState(lastLedger, accounts, HasherFactory.CreateHasher(lastLedger.Ledger.LedgerLight.Version));
            // Debug.Assert(SignedLedgerValidator.Validate(this.lastLedger) == LedgerValidationStatus.Ok, "Last Ledger is not valid"); // Most likely not enough signatures (see quorum)
        }

        public LedgerMerkleRootHash GetMerkleRootHash()
        {
            return GetMerkleRootInternal(LedgerState);
        }

        private LedgerMerkleRootHash GetMerkleRootInternal(ImmutableLedgerState ledgerState)
        {
            // backward compatibility
            var version = ledgerState.LastLedger.Ledger.LedgerLight.Version;
            if (version == ProtocolVersion.InitialVersion)
                return new LedgerMerkleRoot(ledgerState.GetAccounts(), GetDeclarations(ledgerState), merkleLogger, HasherFactory.CreateHasher(version)).Hash;

            return ledgerState.GetHash();
        }

        // for merkle root
        private IEnumerable<TxDeclaration> GetDeclarations(ImmutableLedgerState ledgerState)
        {
            return ledgerState.GetAccounts().Where(account => account.Declaration != null).Select(account => (TxDeclaration) account.Declaration);
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

            Finalize(CreateLedgerState(signed));

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
            return LedgerState.LastLedger.Hash;
        }

        public long GetNextHeight()
        {
            return GetLedgerLight().Height + 1;
        }

        // we create a new ledger state based on the current state and the new ledger
        private MutableLedgerState CreateLedgerState(SignedLedger signedLedger)
        {
            var state = new MutableLedgerState(LedgerState, signedLedger) {AccountCreated = account => LiveService.AccountManager.AddAccount(account.Address, new ExtendedAccount(account))};

            byte index = 0;
            foreach (var signed in signedLedger.Ledger.Block.Transactions)
            {
                // TODO make a better validation
                // Debug.Assert(signed.Transaction.Expire > signedLedger.Ledger.LedgerLight.BeginTime);
                Debug.Assert(signedLedger.Ledger.Block.FeeTransactionIndex == index++ || LiveService.TransactionManager.TransactionValidator.ValidateBalance(state, signed.Transaction.GetInputs()));
                LiveService.SignedTransactionManager.Execute(state, signed.Transaction);
            }

            return state;
        }

        // we finalize the ledger and create a new immutable ledger state
        private void Finalize(MutableLedgerState state)
        {
            var ledgerState = state.Finalize();
            
            if (!CheckMerkleRootInternal(ledgerState))
                throw new Exception("Merkle root is not valid");

            LiveService.PersistenceManager.Save(state.GetLedgerStateChange());

            LedgerState = ledgerState;

            BroadcastNewLedger(LedgerState.LastLedger);
        }

        private bool CheckMerkleRootInternal(ImmutableLedgerState ledgerState)
        {
            var hash = GetMerkleRootInternal(ledgerState);
            var ledger = ledgerState.LastLedger;

            return ledger.Ledger.MerkleHash.Equals(hash);
        }


        public bool CheckMerkleRoot()
        {
            return CheckMerkleRootInternal(LedgerState);
        }

        private void BroadcastNewLedger(SignedLedger signedLedger)
        {
            var message = NotificationHelper.CreateSignedNewLedgerNotification(signedLedger);
            // broadcast the hash of the new ledger with the signature.
            ConnectionService.BlockchainChannel.Broadcast(message);
            logger.Log("Broadcast Signed New Ledger");
        }

        public LedgerLight GetLedgerLight()
        {
            return GetSignedLedger().Ledger.LedgerLight;
        }

        public SignedLedger GetSignedLedger()
        {
            return LedgerState.LastLedger;
        }

        public long GetLedgerBeginTime()
        {
            return GetLedgerLight().Timestamp; // TODO beginTime or beginTime + 1 ?
        }
    }
}