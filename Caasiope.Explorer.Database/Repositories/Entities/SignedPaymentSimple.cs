using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Repositories.Entities
{
    public class SignedTransactionSimple
    {
        public readonly TransactionHash TransactionHash;
        public readonly long LedgerHeight;
        public readonly long Expire;

        public SignedTransactionSimple(TransactionHash transactionHash, long ledgerHeight, long expire)
        {
            TransactionHash = transactionHash;
            LedgerHeight = ledgerHeight;
            Expire = expire;
        }
    }
}