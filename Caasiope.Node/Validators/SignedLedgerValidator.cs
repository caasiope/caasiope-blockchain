using System.Collections.Generic;
using Caasiope.Protocol.Types;
using Caasiope.Protocol.Validators;

namespace Caasiope.Node.Validators
{
    public class SignedLedgerValidator: LedgerRequiredValidatorsFactory
    {
        private readonly HashSet<PublicKey> validators;
        private readonly int quorum;
        private readonly Network network;

        public SignedLedgerValidator(HashSet<PublicKey> validators, int quorum, Network network)
        {
            this.validators = validators;
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