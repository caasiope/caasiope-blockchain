using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Node.Services;
using Caasiope.Protocol.Types;
using Helios.Common.Extensions;
using Caasiope.Node.Managers;
using Caasiope.Node.Types;
using Caasiope.Protocol.Validators;

namespace Caasiope.Node.Validators
{
    public class TransactionValidator
    {
        [Injected] public ILedgerService LedgerService;
        [Injected] public ILiveService LiveService;

        public void Initialize()
        {
            Injector.Inject(this);
        }

        public bool Validate(SignedTransaction signed)
        {
            var timestamp = LedgerService.LedgerManager.GetLedgerBeginTime();
            // validate format
            if (!ValidateFormat(signed))
                return false;

            // validate expiration
            // TODO use end time and not begin time
            if (!ValidateExpiration(signed.Transaction, timestamp))
                return false;

            // validate signature
            if (!signed.CheckSignatures(LiveService.SignatureManager.TransactionRequiredValidationFactory, LedgerService.LedgerManager.Network, timestamp))
                return false;

            return Validate(signed.Transaction);
        }

        private bool ValidateExpiration(Transaction transaction, long timestamp)
        {
            return transaction.Expire > timestamp; // TODO > or >= ?
        }

        protected bool Validate(Transaction transaction)
        {
            // validate Declarations
            if (!ValidateDeclarations(transaction))
                return false;

            // validate input = output
            if (!ValidateZeroSum(transaction))
                return false;

            // validate format
            if (!ValidateFormat(transaction))
                return false;

            return true;
        }

        private bool ValidateFormat(SignedTransaction signed)
        {
            var signatures = signed.Signatures;
            // make sure every address is unique
            if (!NoDuplicate(signatures, (a, b) => a.PublicKey == b.PublicKey))
                return false;
            return true;
        }

        protected bool NoDuplicate<T1>(IReadOnlyList<T1> list, Func<T1, T1, bool> areEqual)
        {
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (i == j) continue;
                    if (areEqual(list[i], list[j]))
                        return false;
                }
            }
            return true;
        }

        private bool ValidateFormat(Transaction transaction)
        {
            // make sure every input/output (address + currency) is unique
            var list = new List<TxInputOutput>();
            list.AddRange(transaction.Outputs);
            list.AddRange(transaction.Inputs);
            if (!NoDuplicate(list, (a, b) => a.Address == b.Address && a.Currency == b.Currency))
                return false;

            // check amount > 0
            if (list.Any(t => t.Amount <= 0))
                return false;

            // Check that fees are positive
            if (transaction.Fees != null && transaction.Fees.Amount <= 0)
                return false;

            //// TODO validate outputs exist
            //// Cannot validate on this state
            //foreach (var output in transaction.Outputs)
            //{
            //    Account account;
            //    if (output.Address.Type != AddressType.ECDSA && !AccountManager.TryGetAccount(output.Address.Encoded, out account))
            //        return false;
            //}

            return true;
        }

        private bool ValidateZeroSum(Transaction transaction)
        {
            var amounts = new Dictionary<Currency, Amount>();

            foreach (var input in transaction.Inputs)
                UpdateAmount(amounts, input.Currency, input.Amount);

            foreach (var input in transaction.Outputs)
                UpdateAmount(amounts, input.Currency, -input.Amount);

            return amounts.Values.All(amount => amount == 0);
        }

        private void UpdateAmount(Dictionary<Currency, Amount> amounts, Currency currency, Amount amount)
        {
            if (amounts.ContainsKey(currency))
                amounts[currency] += amount;
            else
                amounts[currency] = amount;
        }

        private bool ValidateDeclarations(Transaction transaction)
        {
            var valid = true;

            var multisigs = new List<MultiSignature>();
            var hashlocks = new List<HashLock>();
            var timelocks = new List<TimeLock>();
            var secrets = new List<SecretRevelation>();

            foreach (var declaration in transaction.Declarations)
            {
                switch (declaration.Type)
                {
                    case DeclarationType.MultiSignature:
                        var multisig = (MultiSignature) declaration;
                        valid = multisig.Required > 0;
                        multisigs.Add(multisig);
                        // TODO validate addresses ?
                        break;
                    case DeclarationType.HashLock:
                        var hashLock = (HashLock)declaration;
                        hashlocks.Add(hashLock);
                        break;
                    case DeclarationType.Secret:
                        var secret = (SecretRevelation)declaration;
                        valid = secret.Secret?.Bytes != null;
                        secrets.Add(secret);
                        break;
                    case DeclarationType.TimeLock:
                        var timeLock = (TimeLock)declaration;
                        timelocks.Add(timeLock);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            valid &= NoDuplicate(multisigs, (a, b) => a.Equals(b));
            valid &= NoDuplicate(hashlocks, (a, b) => a.Equals(b));
            valid &= NoDuplicate(timelocks, (a, b) => a.Equals(b));
            valid &= NoDuplicate(secrets, (a, b) => a.Equals(b));

            return valid;
        }
        /*
        protected bool Validate(SignedTransaction signed, MultiSignature declaration)
        {
            MultiSignatureAddress multi;
            if (MultiSignatureManager.TryGetAddress(declaration.Address, out multi))
                return false;
            return true;
        }
        */

        public bool ValidateBalance(LedgerState state, IEnumerable<TxInput> inputs)
        {
            // we cannot have duplicate (account + currency). In fact we can since we use fees in input
            var amounts = new Dictionary<string, Amount>();
            foreach (var input in inputs)
            {
                var currency = input.Currency;
                var address = input.Address.Encoded;
                if (!state.TryGetAccount(address, out var account))
                    return false;

                // TODO looks not good
                var amount = amounts[address + Currency.ToSymbol(currency)] = amounts.GetOrCreate(address + Currency.ToSymbol(currency), () => 0) + input.Amount;
                if (!LiveService.IssuerManager.IsIssuer(currency, account.Address) && account.GetBalance(currency) < amount)
                    return false;
            }

            return true;
        }
    }
}
