using System.Collections.Generic;
using Caasiope.Protocol.Types;

namespace Caasiope.Node.Managers
{
    public class TimeLockManager
    {
        private readonly Dictionary<string, TimeLock> timelocks = new Dictionary<string, TimeLock>();

        public void Initialize(List<TimeLockAccount> list)
        {
            Injector.Inject(this);

            foreach (var timelock in list)
            {
                TryAddAccount(new TimeLock(timelock.Timestamp));
            }
        }

        /// <summary>
        /// returns true if account is new
        /// </summary>
        /// <param name="timelock"></param>
        /// <returns>true if account is new</returns>
        public bool TryAddAccount(TimeLock timelock)
        {
            var encoded = timelock.Address.Encoded;

            if (!timelocks.ContainsKey(encoded))
            {
                timelocks.Add(encoded, timelock);
                return true;
            }
            return false;
        }

        public bool TryGetAddress(Address address, out TimeLock timelock)
        {
            if (timelocks.TryGetValue(address.Encoded, out var account))
            {
                timelock = account;
                return true;
            }

            timelock = null;
            return false;
        }

        public IEnumerable<TimeLock> GetTimeLocks()
        {
            return timelocks.Values;
        }
    }
}