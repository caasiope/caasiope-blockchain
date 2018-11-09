using System;
using System.Diagnostics;
using System.Text;
using Helios.Common.Extensions;
using Caasiope.NBitcoin;
using Caasiope.NBitcoin.Crypto;
using Caasiope.Protocol.Formats;

namespace Caasiope.Protocol.Types
{
    public class PrivateKey : Key
    {
        public const int KEY_SIZE = 32;
        // TODO find why we have this
        private static readonly uint256 N = uint256.Parse("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141");
        
        public PrivateKey(byte[] data) : base(data, true)
        {
            Debug.Assert(Check(data));
        }

        private static bool Check(byte[] vch)
        {
            // TODO what are we checking in fact ?
            var candidateKey = new uint256(vch.SubArray(0, KEY_SIZE));
            return candidateKey > 0 && candidateKey < N;
        }

        public static PrivateKey FromBase64(string data)
        {
            return new PrivateKey(Convert.FromBase64String(data));
        }

        // TODO put in extension method
        public PublicKey GetPublicKey()
        {
            return new PublicKey(ECDSAKey.GetPubKey(false));
        }

	    public ECDSASignature Sign(byte[] bytes)
	    {
		    return ECDSAKey.Sign(bytes);
	    }

		public bool Verify(byte[] bytes, ECDSASignature signature)
		{
		    return ECDSAKey.Verify(bytes, signature);
		}
	}
}