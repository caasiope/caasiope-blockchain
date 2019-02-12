using Caasiope.Protocol.Types;
using Caasiope.Protocol;

namespace Caasiope.Explorer.Database.Repositories.Entities
{
    public class HashLockEntity
    {
        public readonly long DeclarationId;
        public readonly HashLockAccount Account;

        public HashLockEntity(long id, HashLockAccount account)
        {
            DeclarationId = id;
            Account = account;
        }
    }
}