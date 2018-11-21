using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories.Entities
{
    public class AccountEntity
    {
        public readonly Address Address;
        public readonly byte[] Raw;

        public AccountEntity(Address address, byte[] raw)
        {
            Address = address;
            Raw = raw;
        }
    }
}
