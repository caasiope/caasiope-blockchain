using System.Collections.Generic;

namespace Caasiope.Protocol.Types
{
    // TODO maybe wrong namespace
    public class LedgerStateChange
    {
        public readonly List<Account> Accounts;
        public readonly List<MultiSignature> MultiSignatures;
        public readonly List<HashLock> HashLocks;
        public readonly List<TimeLock> TimeLocks;

        public LedgerStateChange(List<Account> accounts, List<MultiSignature> multiSignatures, List<HashLock> hashLocks, List<TimeLock> timeLocks)
        {
            Accounts = accounts;
            MultiSignatures = multiSignatures;
            HashLocks = hashLocks;
            TimeLocks = timeLocks;
        }
    }

    public class SignedLedgerState
    {
        public readonly SignedLedger Ledger;
        public readonly LedgerStateChange State;

        public SignedLedgerState(SignedLedger ledger, LedgerStateChange state)
        {
            Ledger = ledger;
            State = state;
        }
    }
}