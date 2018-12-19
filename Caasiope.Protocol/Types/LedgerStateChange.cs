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
        public readonly List<VendingMachine> VendingMachines;

        public LedgerStateChange(List<Account> accounts, List<MultiSignature> multiSignatures, List<HashLock> hashLocks, List<TimeLock> timeLocks, List<VendingMachine> machines)
        {
            Accounts = accounts;
            MultiSignatures = multiSignatures;
            HashLocks = hashLocks;
            TimeLocks = timeLocks;
            VendingMachines = machines;
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