using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Caasiope.Protocol.Types
{
    // TODO maybe wrong namespace
    public class LedgerStateChange
    {
        public readonly List<AccountEntity> Accounts;
        public readonly List<AccountBalanceFull> Balances;
        public readonly List<MultiSignature> MultiSignatures;
        public readonly List<HashLock> HashLocks;
        public readonly List<TimeLock> TimeLocks;

        public LedgerStateChange(List<AccountEntity> accounts, List<AccountBalanceFull> balances, List<MultiSignature> multiSignatures, List<HashLock> hashLocks, List<TimeLock> timeLocks)
        {
            Accounts = accounts;
            Balances = balances;
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