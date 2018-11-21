using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories.Entities
{
    public class AccountEntity
    {
        public readonly Address Address;
        public readonly long CurrentLedgerHeight;
        public readonly bool IsDeclared;

        public AccountEntity(Address address, long currentLedgerHeight, bool isDeclared)
        {
            Address = address;
            CurrentLedgerHeight = currentLedgerHeight;
            IsDeclared = isDeclared;
        }
    }
}
