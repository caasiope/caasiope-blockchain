
using System.Collections.Generic;
using System.Linq;

namespace Caasiope.Protocol.Types
{
    public class SignedNewLedger
    {
        public readonly LedgerHash Hash;
        public readonly long Height;
        public readonly long Timestamp;
        public readonly LedgerHash PreviousLedgerHash;
        public readonly IEnumerable<Signature> Signatures;

        public SignedNewLedger(SignedLedger ledger)
        {
            Hash = ledger.Hash;
            Height = ledger.Ledger.LedgerLight.Height;
            Timestamp = ledger.Ledger.LedgerLight.Timestamp;
            PreviousLedgerHash = ledger.Ledger.LedgerLight.Lastledger; 
            Signatures = ledger.Signatures.ToList();
        }

        public SignedNewLedger(LedgerHash hash,  long height, long timestamp, LedgerHash previousLedgerHash, IEnumerable<Signature> signatures)
        {
            Hash = hash;
            Height = height;
            Timestamp = timestamp;
            PreviousLedgerHash = previousLedgerHash;
            Signatures = signatures;
        }
    }
}