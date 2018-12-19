using Caasiope.Protocol.Types;
using Caasiope.Protocol;

namespace Caasiope.Explorer.Database.Repositories.Entities
{
    public class MultiSignatureEntity
    {
        public readonly long DeclarationId;
        public readonly MultiSignatureHash Hash;
        public readonly MultiSignatureAccount Account;

        public MultiSignatureEntity(long id, MultiSignatureHash hash, MultiSignatureAccount account)
        {
            DeclarationId = id;
            Hash = hash;
            Account = account;
        }
	}
}
