using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.Database.Repositories.Entities
{
    public class VendingMachineEntity
    {
        public readonly long DeclarationId;
        public readonly VendingMachineAccount Account;

        public VendingMachineEntity(long id, VendingMachineAccount account)
        {
            DeclarationId = id;
            Account = account;
        }
    }
}