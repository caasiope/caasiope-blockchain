using System;
using System.Collections.Generic;
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

        public void Initialize(List<Account> list)
        {
            // TODO add the account as the last history
            using (locker.CreateLock())
            {
                foreach (var account in list)
                {
                    accounts.Add(account.Address, new ExtendedAccount());
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
    }

    // represents an account in memory
    public class ExtendedAccount
    {
        public TxAddressDeclaration Declaration { get; set; }

        // make it a node of the account history list
    }
}
