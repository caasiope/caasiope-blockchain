using System.Collections.Generic;
using Caasiope.Protocol.Types;
using Caasiope.Protocol.Validators;

namespace Caasiope.Node.Validators
{
    public class SignedLedgerValidator: LedgerRequiredValidatorsFactory
    {
        private readonly HashSet<PublicKey> validators = new HashSet<PublicKey>();
        private readonly int quorum;
        private readonly Network network;

        public SignedLedgerValidator(IEnumerable<Validator> validators, int quorum, Network network)
        {
            foreach (var validator in validators)
            {
                this.validators.Add(validator.PublicKey);
            }
            this.quorum = quorum;
            this.network = network;
        }

        public LedgerValidationStatus Validate(SignedLedger signed)
        {
            // validate signature
            return signed.CheckSignatures(this, network);
        }

        public LedgerValidationStatus Validate(SignedNewLedger signed)
        {
            // validate signature
            return signed.CheckSignatures(this, network);
        }

        public override LedgerRequiredSignature GetRequiredValidators()
        {
            return new LedgerRequiredSignature(validators, quorum);
        }
    }
}