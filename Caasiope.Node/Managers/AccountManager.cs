using System;
using System.Collections.Generic;
using System.Linq;
using Caasiope.Protocol.Types;
using Helios.Common.Synchronization;

namespace Caasiope.Node.Managers
{
    // this class is in charge to index every account we know
    // every thread is going to concurrently get the reference to the account they need
    // any operation on the tracked account object needs to be lock free

    public class AccountManager
    {
        // account indexed by encoded address
        private readonly Dictionary<Address, ExtendedAccount> accounts = new Dictionary<Address, ExtendedAccount>();
        private readonly MonitorLocker locker = new MonitorLocker();

        public void Initialize(IEnumerable<Account> list)
        {
            // TODO add the account as the last history
            using (locker.CreateLock())
            {
                foreach (var account in list)
                {
                    accounts.Add(account.Address, new ExtendedAccount(account));
                }
            }
        }

        public bool TryGetAccount(Address address, out ExtendedAccount account)
        {
            using (locker.CreateLock())
            {
                return accounts.TryGetValue(address, out account);
            }
        }

        public bool AddAccount(Address address, ExtendedAccount account)
        {
            using (locker.CreateLock())
            {
                if (accounts.ContainsKey(address))
                    return false;
                accounts.Add(address, account);
                return true;
            }
        }

        public ExtendedAccount GetOrCreateAccount(Address address, Func<ExtendedAccount> create)
        {
            using (locker.CreateLock())
            {
                if (!accounts.TryGetValue(address, out var account))
                {
                    account = create();
                    accounts.Add(address, account);
                }

                return account;
            }
        }

        // TODO ugly
        // used only for initialization
        internal Dictionary<Address, Account> GetAccounts()
        {
            var dictionary = new Dictionary<Address, Account>();
            using (locker.CreateLock())
            {
                foreach (var pair in accounts)
                    dictionary.Add(pair.Key, pair.Value.Account);
            }
            return dictionary;
        }
    }

    // represents an account in memory
    public class ExtendedAccount
    {
        // make it a node of the account history list

        // the initial account
        public readonly Account Account;

        public ExtendedAccount(Account account)
        {
            Account = account;
        }
    }
}
