using System.Collections.Generic;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class MultiSignature : TxDeclaration
    {
        public List<string> Signers;
        public short Required;

        public MultiSignature(List<string> signers, short required)
        {
            Signers = signers;
            Required = required;
        }
    }
}