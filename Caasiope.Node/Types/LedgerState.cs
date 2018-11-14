using System;
using Caasiope.Node.Sagas;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Types
{
    public class ImmutableLedgerState : LedgerState
    {
        public readonly SignedLedger LastLedger;

        public ImmutableLedgerState(SignedLedger lastLedger)
        {
            LastLedger = lastLedger;
        }
    }

    public class MutableLedgerState : LedgerState
    {
        public MutableLedgerState(ImmutableLedgerState state)
        {
            // create next state
            // throw new NotImplementedException();
        }

        // TODO remove
        internal FinalizeLedgerBard Bard;
        public SignedLedger SignedLedger { get; set; }

        public SignedLedgerState GetLedgerStateChange()
        {
            return new SignedLedgerState(SignedLedger, Bard.Saga.GetStateChange());
        }

        public ImmutableLedgerState Finalize()
        {
            return new ImmutableLedgerState(SignedLedger);
        }

        public void TryAddAccount(MultiSignature multisig)
        {
            Bard.Saga.TryAddAccount(multisig);
        }

        internal void TryAddAccount(HashLock hashLock)
        {
            Bard.Saga.TryAddAccount(hashLock);
        }

        internal void TryAddAccount(TimeLock timeLock)
        {
            Bard.Saga.TryAddAccount(timeLock);
        }

        internal void AddAccount(Account account)
        {
            Bard.Saga.AddAccount(account);
        }

        internal void SetBalance(Account account, Currency currency, Amount amount)
        {
            Bard.Saga.SetBalance(account, currency, amount);
        }
    }

    public class LedgerState
    {
        public bool TryGetAccount(string address, out Account account)
        {
            throw new NotImplementedException();
        }
    }
}
