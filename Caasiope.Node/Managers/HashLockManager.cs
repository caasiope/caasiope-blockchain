using System;
using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    public class HashLockManager
    {
        private readonly Dictionary<string, HashLock> hashlocks = new Dictionary<string, HashLock>();

        public void Initialize(List<HashLockAccount> list)
        {
            Injector.Inject(this);

            foreach (var hashlock in list)
            {
                if(!TryAddAccount(new HashLock(hashlock.SecretHash)))
                    throw new ArgumentException("list should be empty at initialization");
            }
        }

        /// <summary>
        /// returns true if account is new
        /// </summary>
        /// <param name="hashlock"></param>
        /// <returns>true if account is new</returns>
        public bool TryAddAccount(HashLock hashlock)
        {
            var encoded = hashlock.Address.Encoded;

            if (!hashlocks.ContainsKey(encoded))
            {
                hashlocks.Add(encoded, hashlock);
                return true;
            }

            return false;
        }

        public bool TryGetAddress(Address address, out HashLock hashlock)
        {
            if (hashlocks.TryGetValue(address.Encoded, out var account))
            {
                hashlock = account;
                return true;
            }

            hashlock = null;
            return false;
        }

        public IEnumerable<HashLock> GetHashLocks()
        {
            return hashlocks.Values;
        }
    }
}