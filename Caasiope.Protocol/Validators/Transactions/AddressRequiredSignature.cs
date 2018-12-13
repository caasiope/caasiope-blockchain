using System.Collections.Generic;
using System.Diagnostics;
using Caasiope.Protocol.Types;

namespace Caasiope.Protocol.Validators.Transactions
{
    public class AddressRequiredSignature : TransactionRequiredValidation
    {
        public readonly Address Address;

        public AddressRequiredSignature(Address address)
        {
            Debug.Assert(address.Type == AddressType.ECDSA);
            Address = address;
        }

        public override bool IsValid(List<TransactionValidationEngine.SignatureRequired> signatures, Transaction transaction, long timestamp)
        {
            // try to find a signature where the public key matches with the required address
            foreach (var signature in signatures)
            {
                if (signature.CheckAddress(Address))
                {
                    signature.Require();
                    return true;
                }
            }
            return false;
        }
    }
}