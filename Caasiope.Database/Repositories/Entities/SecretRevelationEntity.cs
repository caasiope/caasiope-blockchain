using Caasiope.Protocol.Types;

namespace Caasiope.Database.Repositories.Entities
{
    public class SecretRevelationEntity
    {
        public readonly long DeclarationId;
        public readonly SecretRevelation SecretRevelation;

        public SecretRevelationEntity(long id, SecretRevelation secret)
        {
            DeclarationId = id;
            SecretRevelation = secret;
        }
    }
}