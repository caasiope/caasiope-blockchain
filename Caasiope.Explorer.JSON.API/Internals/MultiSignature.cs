using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class MultiSignature : TxDeclaration
    {
        public List<string> Signers;
        public short Required;

        public MultiSignature(List<string> signers, short required) : this()
        {
            Signers = signers;
            Required = required;
        }

        public MultiSignature() : base((byte)DeclarationType.MultiSignature) { }
    }
}