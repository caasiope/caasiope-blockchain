using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Protocol.Validators
{
    public enum LedgerValidationStatus
    {
        Ok,
        InvalidPublicKey,
        NotEnoughSignatures
    }

    public abstract class LedgerRequiredValidatorsFactory
    {
        public abstract LedgerRequiredSignature GetRequiredValidators();
    }

    public abstract class RequiredSignatureFactory
    {
        public abstract bool TryCreateRequiredSignature(Address address, IEnumerable<TxDeclaration> declarations, out RequiredSignature signature);
    }

    public abstract class RequiredSignature
    {
        public abstract bool IsValid();

        public virtual void OnValidSignature(PublicKey key) { }
        // TODO not related to signatures
        public virtual void OnValidDeclaration(TxDeclaration key) { }
        public virtual void OnTimeStamp(long timestamp) { }
    }

    public class LedgerRequiredSignature : RequiredSignature
    {
        private readonly List<PublicKey> signers = new List<PublicKey>();

        private readonly HashSet<PublicKey> validators;
        private readonly int quorum;

        public LedgerRequiredSignature(HashSet<PublicKey> validators, int quorum)
        {
            this.quorum = quorum;
            this.validators = validators;
        }

        public override void OnValidSignature(PublicKey publicKey)
        {
            signers.Add(publicKey);
        }

        public override bool IsValid()
        {
            var keys = 0;

            foreach (var validator in validators)
            {
                if (signers.Contains(validator))
                    keys++;
            }

            return keys >= quorum;
        }
    }
}
