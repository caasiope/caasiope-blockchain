namespace Caasiope.Protocol.Types
{
    public class MultiSignatureSigner
    {
        public readonly Address MultiSignature;
        public readonly Address Signer;

        public MultiSignatureSigner(Address multiSignature, Address signer)
        {
            MultiSignature = multiSignature;
            Signer = signer;
        }
    }
}