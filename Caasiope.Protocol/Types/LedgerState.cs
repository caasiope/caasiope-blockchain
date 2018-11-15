using System.Collections.Generic;
using Caasiope.Protocol.Extensions;

namespace Caasiope.Protocol.Types
{
    public class ImmutableLedgerState : LedgerState
    {
        public readonly SignedLedger LastLedger;
        public long Height => LastLedger.Ledger.LedgerLight.Height;

        public ImmutableLedgerState(SignedLedger lastLedger, Dictionary<Address, Account> list)
        {
            LastLedger = lastLedger;
            accounts = list;
        }

        private readonly Dictionary<Address, Account> accounts;

        public IEnumerable<Account> GetAccounts()
        {
            // TODO Optimize
            return accounts.Values;
        }

        public override bool TryGetAccount(Address address, out Account account)
        {
            return accounts.TryGetValue(address, out account);
        }
    }

    // TODO move in Node project
    public abstract class LedgerState
    {
        public abstract bool TryGetAccount(Address address, out Account account);

        public bool TryGetDeclaration<T>(Address address, out T declaration) where T : TxAddressDeclaration
        {
            // if the account does exists in the state
            if (!TryGetAccount(address, out var account))
            {
                declaration = null;
                return false;
            }

            // check the account has been declared
            declaration = account.GetDeclaration<T>();
            return declaration != null;
        }
    }
}
