using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories.Entities
{
    public class LedgerSignature
    {
        public readonly Signature Signature;
        public readonly long LedgerHeight;

        public LedgerSignature(Signature signature, long ledgerHeight)
        {
            Signature = signature;
            LedgerHeight = ledgerHeight;
        }
    }
}