using System;
using System.Collections.Generic;
using Caasiope.Node;
using Caasiope.Node.Processors.Commands;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Caasiope.Wallet.Listeners;
using Caasiope.Wallet.Managers;
using Helios.Common.Concepts.Services;
using Helios.Common.Synchronization;
using Caasiope.Protocol;
using ResultCode = Caasiope.Node.ResultCode;

namespace Caasiope.Wallet.Services
{
    public interface IWalletService : IService
    {
        PrepareTransactionManager PrepareTransactionManager { get; }
        AddressListener AddressListener { get; }
        AliasManager AliasManager { get; }
        TransactionSubmissionListener TransactionSubmissionListener { get; }

        bool ImportPrivateKey(string label, PrivateKeyNotWallet key);
        bool SetActiveKey(string alias);
        Aliased<PrivateKeyNotWallet> GetActiveKey();
        bool SignAndSubmit(Transaction transaction);
        IEnumerable<Aliased<PrivateKeyNotWallet>> GetPrivateKeys();
        void SetDefaultFees(Currency currency, Amount amount);
        TxInput CreateFeesInput(Address payer);
        bool ImportDeclaration(string alias, TxDeclaration declaration);
        bool TryGetDeclaration(string label, out TxDeclaration declaration);
        Address GetAddress(string text);
    }

    public class WalletService : Node.Services.ThreadedService, IWalletService
    {
        private readonly Dictionary<string, Aliased<PrivateKeyNotWallet>> wallets = new Dictionary<string, Aliased<PrivateKeyNotWallet>>();
        private readonly Dictionary<string, TxDeclaration> declarations = new Dictionary<string, TxDeclaration>();
        
        [Injected] public ILiveService LiveService;
        [Injected] public ILedgerService LedgerService;

        private Aliased<PrivateKeyNotWallet> active;

        private SynchronizedBlockingState<LedgerStatus>.Listener state;

        public PrepareTransactionManager PrepareTransactionManager { get; private set; }
        public AddressListener AddressListener { get; private set; }
        public AliasManager AliasManager { get; private set; }
        public TransactionSubmissionListener TransactionSubmissionListener { get; private set; }
        
        public WalletService()
        {
            AddressListener = new AddressListener();
            AliasManager = new AliasManager();
            TransactionSubmissionListener = new TransactionSubmissionListener();
        }

        protected override void OnInitialize()
        {
            PrepareTransactionManager = Injector.Inject(new PrepareTransactionManager());

            state = LedgerService.State.CreateListener();
            RegisterWaitHandle(state.CancelEvent, HandleLedgerStateChanged, true);
            // load keys from file
        }

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override void Run()
        {
            if (state.State == LedgerStatus.Updated)
            {
                var ledger = LedgerService.LedgerManager.GetSignedLedger();
                AddressListener.OnLedgerUpdated(ledger);
                TransactionSubmissionListener.OnLedgerUpdated(ledger.Ledger);
            }
        }

        public bool ImportPrivateKey(string label, PrivateKeyNotWallet key)
        {
            var aliased = AliasManager.SetAlias(label, key);
            var address = key.Account.Address.Encoded;
            if (wallets.ContainsKey(label))
                return false;
            wallets.Add(address, aliased);
            AddressListener.Listen(key.Account.Address);
            return true;
        }

        public bool SetActiveKey(string alias)
        {
            if (!AliasManager.TryGetByAlias(alias, out Aliased<PrivateKeyNotWallet> wallet))
                return false;
            active = wallet;
            return true;
        }

        public Aliased<PrivateKeyNotWallet> GetActiveKey()
        {
            return active;
        }

        public bool SignAndSubmit(Transaction transaction)
        {
            var signed = new SignedTransaction(transaction);
            SignTransaction(signed, LedgerService.LedgerManager.Network);
            var command = LiveService.AddCommand(new SendTransactionCommand(signed, (response, code) => {}));
            var output = command.GetOutput();
            var isSuccess = command.ResultCode == ResultCode.Success;

            if (isSuccess)
            {
                TransactionSubmissionListener.OnSubmitted(signed);
            }

            return isSuccess;
        }

        // TODO make something smart that works with declarations
        private void SignTransaction(SignedTransaction signed, Network network)
        {
            foreach (var input in signed.Transaction.GetInputs())
            {
                Aliased<PrivateKeyNotWallet> wallet;
                if(wallets.TryGetValue(input.Address.Encoded, out wallet))
                        wallet.Data.SignTransaction(signed, network);
            }

            //add declaration if needed
        }

        public IEnumerable<Aliased<PrivateKeyNotWallet>> GetPrivateKeys()
        {
            return wallets.Values;
        }

        private AccountBalance fees;
        
        public void SetDefaultFees(Currency currency, Amount amount)
        {
            fees = new AccountBalance(currency, amount);
        }

        public TxInput CreateFeesInput(Address payer)
        {
            return fees == null ? null : new TxInput(payer, fees.Currency, fees.Amount);
        }

        public bool ImportDeclaration(string alias, TxDeclaration declaration)
        {
            AliasManager.SetAlias(alias, declaration);
            if (declarations.ContainsKey(alias))
                return false;
            declarations.Add(alias, declaration);

            if (declaration is TxAddressDeclaration)
            {
                var address = ((TxAddressDeclaration)declaration).Address;
                AliasManager.SetAlias(address.Encoded, declaration);
                if (declarations.ContainsKey(address.Encoded))
                    return false;
                declarations.Add(address.Encoded, declaration);

                AddressListener.Listen(address);
            }
            return true;
        }

        public bool TryGetDeclaration(string label, out TxDeclaration declaration)
        {
            return declarations.TryGetValue(label, out declaration);
        }
        
        public Address GetAddress(string text)
        {
            var ret = GetAddress_(text);
            if (ret == null)
                throw new FormatException($"{text} is not a valid address or alias");
            return ret;
        }

        // the other way to do this, is to index addresses per alias when the item is aliased
        public Address GetAddress_(string text)
        {
            if (text.Length == Address.ENCODED_SIZE)
                return new Address(text);

            IAliased item;
            if (!AliasManager.TryGetByAlias(text, out item))
                return null;

            var declaration = item.GetObject<TxAddressDeclaration>();
            if (declaration != null)
            {
                return declaration.Address;
            }

            var wallet = item.GetObject<PrivateKeyNotWallet>();
            if (wallet != null)
            {
                return wallet.Account.Address;
            }

            return null;
        }

        private void HandleLedgerStateChanged()
        {
            state.NextState();
        }
    }
}
