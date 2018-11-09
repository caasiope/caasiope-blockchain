namespace Caasiope.Protocol.Types
{
    public class Signature
    {
        public readonly PublicKey PublicKey;
        public readonly SignatureByte SignatureByte;

        public Signature(PublicKey publicKey, SignatureByte signatureByte)
        {
            PublicKey = publicKey;
            SignatureByte = signatureByte;
        }
    }
}