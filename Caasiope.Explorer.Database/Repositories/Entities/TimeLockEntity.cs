using Caasiope.Protocol.Types;
using Caasiope.Protocol;

namespace Caasiope.Database.Repositories.Entities
{
    public class TimeLockEntity
    {
        public readonly long DeclarationId;
        public readonly TimeLockAccount Account;

        public TimeLockEntity(long id, TimeLockAccount account)
        {
            DeclarationId = id;
            Account = account;
        }
    }
}