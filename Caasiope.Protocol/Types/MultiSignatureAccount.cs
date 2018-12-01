using System.Collections.Generic;

namespace Caasiope.Protocol.Types
{
    public class MultiSignatureAccount
    {
        public readonly Address Address;
        public readonly short Required;

        public MultiSignatureAccount(Address address, short required)
        {
            Address = address;
            Required = required;
        }
    }
}