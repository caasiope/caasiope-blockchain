using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories.Entities
{
    public class AccountEntity
    {
        public readonly Address Address;
        public readonly byte[] Raw;
        public readonly bool IsNew;

        public AccountEntity(Address address, byte[] raw, bool isNew = false)
        {
            Address = address;
            Raw = raw;
            IsNew = isNew;
        }
    }
}
