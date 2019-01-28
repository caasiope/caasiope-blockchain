using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class MultiSignature : TxAddressDeclaration
    {
        public List<string> Signers;
        public short Required;

        public MultiSignature(List<string> signers, short required, string address) : this()
        {
            Signers = signers;
            Required = required;
            Address = address;
        }

        public MultiSignature() : base((byte)DeclarationType.MultiSignature) { }
    }
}