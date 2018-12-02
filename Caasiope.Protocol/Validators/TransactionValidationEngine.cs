using System.Collections.Generic;
using System.Linq;
using Caasiope.Protocol.Types;

namespace Caasiope.Protocol.Validators
{
    public static class TransactionValidationEngine
    {
        public class SignatureRequired
        {
            private readonly Signature signature;
            public bool Required { get; private set; }

            public SignatureRequired(Signature signature)
            {
                this.signature = signature;
            }

            public void Require()
            {
                Required = true;
            }

            public bool Verify(TransactionHash hash, Network network)
            {
                return signature.PublicKey.Verify(hash, signature.SignatureByte, network);
            }

            public bool CheckAddress(Address address)
            {
                return signature.PublicKey.CheckAddress(address.Encoded);
            }
        }

        public static bool CheckSignatures(this SignedTransaction transaction, TransactionRequiredValidationFactory factory, Network network, long timestamp, ILedgerState state = null)
        {
            var hash = transaction.Hash;

            // get required signatures
            List<TransactionRequiredValidation> requireds;
            if (!TryGetRequiredValidations(state, factory, transaction.Transaction, out requireds))
                return false;

            var signatures = transaction.Signatures.Select(signature => new SignatureRequired(signature)).ToList();

            // check required signatures and declarations are set
            foreach (var required in requireds)
            {
                if (!required.IsValid(signatures, transaction.Transaction, timestamp))
                {
                    return false;
                }
            }

            // TODO we dont accept transactions with useless declarations

            // we try to optimize by checking signatures only at the end
            // verify signatures
            foreach (var signature in signatures)
            {
                // we dont accept transactions with useless signatures
                if (!signature.Required || !signature.Verify(hash, network))
                    return false;
            }

            return true;
        }

        private static bool TryGetRequiredValidations(ILedgerState state, TransactionRequiredValidationFactory factory, Transaction transaction, out List<TransactionRequiredValidation> validations)
        {
            var dictionary = new Dictionary<Address, TransactionRequiredValidation>();
            foreach (var input in transaction.GetInputs())
            {
                var address = input.Address;
                if (!dictionary.ContainsKey(address))
                {
                    TransactionRequiredValidation validation;
                    if (!factory.TryGetRequiredValidations(state, input.Address, transaction.Declarations, out validation))
                    {
                        validations = new List<TransactionRequiredValidation>();
                        return false;
                    }
                    dictionary.Add(address, validation);
                }
            }
            validations = dictionary.Values.ToList();
            return true;
        }

        public static LedgerValidationStatus CheckSignatures(this SignedLedger ledger, LedgerRequiredValidatorsFactory factory, Network network)
        {
            var hash = ledger.Hash;
            // get required signatures
            var required = factory.GetRequiredValidators();

            // verify signatures
            foreach (var signature in ledger.Signatures)
            {
                if (!signature.PublicKey.Verify(hash, signature.SignatureByte, network))
                {
                    return LedgerValidationStatus.InvalidPublicKey;
                }

                required.OnValidSignature(signature.PublicKey);
            }

            // check required signatures are set
            if (!required.IsValid())
                return LedgerValidationStatus.NotEnoughSignatures;
            return LedgerValidationStatus.Ok;
        }

        public static LedgerValidationStatus CheckSignatures(this SignedNewLedger ledger, LedgerRequiredValidatorsFactory factory, Network network)
        {
            var hash = ledger.Hash;

            // get required signatures
            var required = factory.GetRequiredValidators();

            // verify signatures
            foreach (var signature in ledger.Signatures)
            {
                if (!signature.PublicKey.Verify(hash, signature.SignatureByte, network))
                    return LedgerValidationStatus.InvalidPublicKey;

                required.OnValidSignature(signature.PublicKey);
            }

            // check required signatures are set
            if (!required.IsValid())
                return LedgerValidationStatus.NotEnoughSignatures;
            return LedgerValidationStatus.Ok;
        }

        private static bool TryGetRequiredSignatures(ILedgerState state, TransactionRequiredValidationFactory factory, Transaction transaction, out List<TransactionRequiredValidation> list)
        {
            var validations = new Dictionary<string, TransactionRequiredValidation>();

            foreach (var input in transaction.GetInputs())
            {
                if (!validations.ContainsKey(input.Address.Encoded))
                {
                    TransactionRequiredValidation required;
                    if (!factory.TryGetRequiredValidations(state, input.Address, transaction.Declarations, out required))
                    {
                        list = null;
                        return false;
                    }
                    validations.Add(input.Address.Encoded, required);
                }
            }

            list = validations.Values.ToList();
            return true;
        }
    }

    public abstract class TransactionRequiredValidationFactory
    {
        public abstract bool TryGetRequiredValidations(ILedgerState state, Address address, List<TxDeclaration> declarations, out TransactionRequiredValidation required);
    }

    // TODO have one instance per type
    public abstract class TransactionRequiredValidation
    {
        public abstract bool IsValid(List<TransactionValidationEngine.SignatureRequired> signatures, Transaction transaction, long timestamp);
    }

    public class NotDeclaredRequiredValidation : TransactionRequiredValidation
    {
        public static readonly NotDeclaredRequiredValidation Instance = new NotDeclaredRequiredValidation();

        public override bool IsValid(List<TransactionValidationEngine.SignatureRequired> signatures, Transaction transaction, long timestamp)
        {
            return false;
        }
    }
}