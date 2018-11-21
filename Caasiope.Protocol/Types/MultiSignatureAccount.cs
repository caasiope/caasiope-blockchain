using System.Collections.Generic;

namespace Caasiope.Protocol.Types
{
    public class MultiSignatureAccount
    {
        public readonly Address Address;
        public readonly short Required;
        public readonly IEnumerable<Address> multiSignatureSigners;

        public MultiSignatureAccount(Address address, short required, IEnumerable<Address> multiSignatureSigners)
        {
            Address = address;
            Required = required;
            this.multiSignatureSigners = multiSignatureSigners;
        }
    }
}