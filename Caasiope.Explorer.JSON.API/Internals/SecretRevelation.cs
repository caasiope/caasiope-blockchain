namespace Caasiope.Explorer.JSON.API.Internals
{
    public class SecretRevelation : TxDeclaration
    {
        public readonly string Secret;

        public SecretRevelation(string secret)
        {
            Secret = secret;
        }
    }
}