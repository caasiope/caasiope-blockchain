using Caasiope.Protocol.Types;
using Caasiope.Protocol;

namespace Caasiope.Database.Repositories.Entities
{
    public class TxDeclarationEntity
    {
        public readonly Address Address;
        public readonly byte[] Raw;

        public TxDeclarationEntity(Address address, byte[] raw)
        {
            Address = address;
            Raw = raw;
        }
	}
}
