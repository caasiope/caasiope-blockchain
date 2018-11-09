using System.Collections.Generic;
using System.Linq;

namespace Caasiope.Protocol.Types
{
    public class MultiSignatureAddress
    {
        public readonly Address Address;
        public readonly List<Address> Signers;
        public readonly short Required;

        public MultiSignatureAddress(Address address, List<Address> signers, short required)
        {
            Address = address;
            Signers = signers;
            Required = required;
        }

        public static MultiSignatureAddress FromMultiSignature(MultiSignature transaction)
        {
            return new MultiSignatureAddress(transaction.Address, transaction.Signers.ToList(), transaction.Required);
        }
    }
}