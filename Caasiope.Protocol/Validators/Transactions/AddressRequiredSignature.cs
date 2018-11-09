using System.Collections.Generic;
using System.Diagnostics;
using Caasiope.Protocol.Types;

namespace Caasiope.Protocol.Validators.Transactions
{
    public class AddressRequiredSignature : TransactionRequiredValidation
    {
        private readonly Address address;

        public AddressRequiredSignature(Address address)
        {
            Debug.Assert(address.Type == AddressType.ECDSA);
            this.address = address;
        }

        public override bool IsValid(List<TransactionValidationEngine.SignatureRequired> signatures, List<TxDeclaration> declarations, long timestamp)
        {
            // try to find a signature where the public key matches with the required address
            foreach (var signature in signatures)
            {
                if (signature.CheckAddress(address))
                {
                    signature.Require();
                    return true;
                }
            }
            return false;
        }
    }
}