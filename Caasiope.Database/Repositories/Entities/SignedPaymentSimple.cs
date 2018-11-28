using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories.Entities
{
    public class SignedTransactionSimple
    {
        public readonly TransactionHash TransactionHash;
        public readonly long LedgerHeight;
        public readonly long LedgerTimestamp;
        public readonly long Expire;

        public SignedTransactionSimple(TransactionHash transactionHash, long ledgerHeight, long expire, long ledgerTimestamp)
        {
            TransactionHash = transactionHash;
            LedgerHeight = ledgerHeight;
            Expire = expire;
            LedgerTimestamp = ledgerTimestamp;
        }
    }
}