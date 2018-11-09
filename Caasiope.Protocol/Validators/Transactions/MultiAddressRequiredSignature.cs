using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Protocol.Validators.Transactions
{
    // make recursive validation
    public class MultiAddressRequiredSignature : TransactionRequiredValidation
    {
        private readonly List<TransactionRequiredValidation> signers;
        private readonly int threshold;

        public MultiAddressRequiredSignature(MultiSignatureAddress multi, List<TransactionRequiredValidation> signers)
        {
            this.signers = signers;
            threshold = multi.Required;
        }

        public override bool IsValid(List<TransactionValidationEngine.SignatureRequired> signatures, List<TxDeclaration> declarations, long timestamp)
        {
            var count = 0;
            foreach (var required in signers)
            {
                if(required.IsValid(signatures, declarations, timestamp))
                    count++;
            }
            return count >= threshold;
        }
    }
}