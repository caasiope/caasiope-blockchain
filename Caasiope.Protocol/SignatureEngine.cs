using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Caasiope.Protocol.Formats;
using Caasiope.Protocol.Types;
using Caasiope.NBitcoin;
using Caasiope.NBitcoin.Crypto;

namespace Caasiope.Protocol
{

    public static class SignatureEngine
    {
        public static bool CheckSignature(this PublicKey key, string message, string signed, Network network)
        {
            var hash = Hashes.Hash256(Encoding.UTF8.GetBytes(message));
            return key.Verify(hash, new SignatureByte(Convert.FromBase64String(signed)), network);
        }

        public static Signature CreateSignature(this PrivateKeyNotWallet account, Hash256 hash, Network network)
        {
            var signature = account.PrivateKey.SignMessage(hash.Bytes, network);
            Debug.Assert(account.PublicKey.Verify(hash, signature, network));
            return new Signature(account.PublicKey, signature);
        }

        public static bool SignTransaction(this PrivateKeyNotWallet account, SignedTransaction signed, Network network)
        {
            return signed.AddSignature(account.CreateSignature(signed.Hash, network));
        }

        public static bool SignLedger(this PrivateKeyNotWallet account, SignedLedger signed, Network network)
        {
            return signed.AddSignature(account.CreateSignature(signed.Hash, network));
        }

	    public static byte[] FormatMessageForSigning(byte[] messageBytes, Network network)
	    {
		    using (var ms = new MemoryStream())
		    {
			    var header = network.GetVersionBytes(Network.VersionByte.SIGNED_MESSAGE_HEADER_BYTES);
			    ms.Write(header, 0, header.Length);
			    ms.Write(messageBytes, 0, messageBytes.Length);
			    return ms.ToArray();
		    }
	    }
    }

    public static class SignatureExtensions
    {
        // TODO put in extension method
        public static string SignMessage(this PrivateKey key, string message, Network network)
        {
            var hash = Hashes.Hash256(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(SignMessage(key, hash.ToBytes(), network).Bytes);
        }

        public static SignatureByte SignMessage(this PrivateKey key, byte[] hash, Network network)
        {
            // TODO add a prefix to prevent cross protocol signature
            var data = SignatureEngine.FormatMessageForSigning(hash, network);
            // TODO NBitcoin.Key.SignCompact
            var signature = key.Sign(data);
            Debug.Assert(key.Verify(data, signature));
            return SignatureFormat.ToBytes(signature);
        }
    }
}
