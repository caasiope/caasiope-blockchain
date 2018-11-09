using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caasiope.Node.Sagas;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    public class AccountManager : IAccountList
    {
        // account indexed by encoded address
        private readonly Dictionary<string, Account> accounts = new Dictionary<string, Account>();

        // populate accounts fom the database
        public void Initialize(List<Account> list)
        {
            Debug.Assert(accounts.Count == 0);
            foreach (var account in list)
                accounts.Add(account.Address.Encoded, account);
        }

        // get account if it exists in the current state of the ledger
        public bool TryGetAccount(string address, out Account account)
        {
            return accounts.TryGetValue(address, out account);
        }

        // create ecdsa account doesnt change state
        public Account CreateECDSAAccount(Address address)
        {
            Debug.Assert(address.Type == AddressType.ECDSA);
            var encoded = address.Encoded;
            Debug.Assert(!accounts.ContainsKey(encoded));
            return Account.FromAddress(address); 
        }

        // create multisignature account doesnt change state
        public Account CreateMultisignatureECDSAAccount(Address address)
        {
            Debug.Assert(address.Type == AddressType.MultiSignatureECDSA);
            var encoded = address.Encoded;
            Debug.Assert(!accounts.ContainsKey(encoded));
            return Account.FromAddress(address);
        }

        // Create HashLock Account doesnt change state
        public Account CreateHashLockAccount(Address address)
        {
            Debug.Assert(address.Type == AddressType.HashLock);
            var encoded = address.Encoded;
            Debug.Assert(!accounts.ContainsKey(encoded));
            return Account.FromAddress(address);
        }

        // Create TimeLock Account doesnt change state
        public Account CreateTimeLockAccount(Address address)
        {
            Debug.Assert(address.Type == AddressType.TimeLock);
            var encoded = address.Encoded;
            Debug.Assert(!accounts.ContainsKey(encoded));
            return Account.FromAddress(address);
        }

        // add an account to the current state of the ledger
        public void AddAccount(Account account)
        {
            accounts.Add(account.Address.Encoded, account);
        }

        public IEnumerable<Account> GetAccounts()
        {
            // TODO Optimize
            return accounts.Values.Where(account => account.Balances.Any(balance => balance.Amount != 0));
        }
    }
}
