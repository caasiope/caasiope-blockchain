using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class SecretRevelation : TxDeclaration
    {
        public string Secret;

        public SecretRevelation(string secret) : this()
        {
            Secret = secret;
        }

        public SecretRevelation() : base((byte)DeclarationType.Secret) { }
    }
}