
namespace Caasiope.Explorer.Database.Repositories.Entities
{
    public class LedgerStateChangeSimple
    {
        public readonly long LedgerHeight;
        public readonly byte[] Raw;

        public LedgerStateChangeSimple(long ledgerHeight, byte[] raw)
        {
            LedgerHeight = ledgerHeight;
            Raw = raw;
        }
    }
}