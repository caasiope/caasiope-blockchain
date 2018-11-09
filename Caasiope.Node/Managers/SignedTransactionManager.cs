using System;
using System.Diagnostics;
using Caasiope.Node.Sagas;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    public class SignedTransactionManager
    {
        [Injected] public ILiveService LiveService;

        public void Initialize()
        {
            Injector.Inject(this);
        }

        public void Execute(IUpdateStateSaga saga, Transaction transaction)
        {
            // TODO declarations
            foreach (var declaration in transaction.Declarations)
                ApplyDeclaration(saga, declaration);

            // update balances
            foreach (var input in transaction.Inputs)
                UpdateBalance(saga, input.Address, input.Currency, -input.Amount);
            foreach (var input in transaction.Outputs)
                UpdateBalance(saga, input.Address, input.Currency, input.Amount);

            if (transaction.Fees != null)
            {
                UpdateBalance(saga, transaction.Fees.Address, transaction.Fees.Currency, -transaction.Fees.Amount);
            }
        }

        private void ApplyDeclaration(IUpdateStateSaga saga, TxDeclaration declaration)
        {
            if (declaration.Type == DeclarationType.MultiSignature)
            {
                var multisig = (MultiSignature)declaration;
                saga.TryAddAccount(multisig);
            }
            else if (declaration.Type == DeclarationType.HashLock)
            {
                var hashLock = (HashLock)declaration;
                saga.TryAddAccount(hashLock);
            }
            else if(declaration.Type == DeclarationType.TimeLock)
            {
                var timeLock = (TimeLock)declaration;
                saga.TryAddAccount(timeLock);
            }
        }

        private void UpdateBalance(IUpdateStateSaga saga, Address address, Currency currency, Amount amount)
        {
            if (!saga.TryGetAccount(address.Encoded, out var account))
            {
                //Debug.Assert(address.Type == AddressType.ECDSA);
                Debug.Assert(amount != 0);
                if(address.Type == AddressType.ECDSA)
                    account = LiveService.AccountManager.CreateECDSAAccount(address);
                else if (address.Type == AddressType.MultiSignatureECDSA)
                    account = LiveService.AccountManager.CreateMultisignatureECDSAAccount(address);
                else if (address.Type == AddressType.HashLock)
                    account = LiveService.AccountManager.CreateHashLockAccount(address);
                else if (address.Type == AddressType.TimeLock)
                    account = LiveService.AccountManager.CreateTimeLockAccount(address);
                else 
                    throw new NotImplementedException();
                saga.AddAccount(account);
            }

            var balance = account.GetBalance(currency);

            Debug.Assert(LiveService.IssuerManager.IsIssuer(currency, address) || balance + amount >= 0);
            saga.SetBalance(account, currency, balance + amount);
        }
        /*
        public bool Validate(SignedTransaction signed)
        {
            if (!Validate(signed))
                return false;

            // validate balance
            if (!ValidateBalance(saga, signed.Transaction))
                return false;

            return true;
        }
        */
        /*
        public static bool Validate(IAccountList accounts, SignedTransaction signed)
        {
            if (!Validate(signed))
                return false;

            // validate balance
            if (!ValidateBalance(accounts, signed.Transaction.GetInputs()))
                return false;

            return true;
        }
        */
    }
}