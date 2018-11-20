using System.Collections.Generic;
using Caasiope.Protocol.MerkleTrees;
using HashLib;
using Caasiope.NBitcoin;
using Caasiope.Protocol.Compression;

namespace Caasiope.Protocol.Types
{
    public class LedgerLight
    {
        public readonly long Height;
        public readonly long Timestamp;
        public readonly LedgerHash Lastledger;
        public readonly ProtocolVersion Version;

        public LedgerLight(long height, long timestamp, LedgerHash lastledger, ProtocolVersion version)
        {
            Height = height;
            Timestamp = timestamp;
            Lastledger = lastledger;
            Version = version;
        }
    }

    public class Ledger
    {
        public readonly LedgerMerkleRootHash MerkleHash;
        public readonly LedgerLight LedgerLight;
        public readonly Block Block;

        public Ledger(LedgerLight light, Block block, LedgerMerkleRootHash merkleHash)
        {
            LedgerLight = light;
            Block = block;
            MerkleHash = merkleHash;
        }

        public LedgerHash GetHash()
        {
            using (var stream = new ByteStream())
            {
                stream.Write(this);
                var message = stream.GetBytes();

                var hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
                var hash = hasher.ComputeBytes(message).GetBytes();
                return new LedgerHash(hash);
            }
        }
    }

    public class ProtocolVersion
    {
        public readonly byte VersionNumber;

        public ProtocolVersion(byte version)
        {
            VersionNumber = version;
        }

        public static ProtocolVersion InitialVersion = new ProtocolVersion(0x1);
        public static ProtocolVersion ImmutableState = new ProtocolVersion(0x2);

        public static bool operator ==(ProtocolVersion a, ProtocolVersion b)
        {
            return a.VersionNumber == b.VersionNumber;
        }

        public static bool operator !=(ProtocolVersion a, ProtocolVersion b)
        {
            return !(a == b);
        }
    }

    public class LedgerHash : Hash256 { public LedgerHash(byte[] bytes) : base(bytes) { }}

    public class SignedLedger : Signed<Ledger, LedgerHash>
    {
        public Ledger Ledger => Data;

        public SignedLedger(Ledger ledger) : base(ledger, ledger.GetHash(), new List<Signature>()) { }
        public SignedLedger(Ledger ledger, List<Signature> signatures) : base(ledger, ledger.GetHash(), signatures) { }
    }

    public static class LedgerCompressionEngine
    {
        public static byte[] ZipSignedLedger(SignedLedger signedLedger)
        {
            using (var stream = new ByteStream())
            {
                stream.Write(signedLedger);
                return Zipper.Zip(stream.GetBytes());
            }
        }

        public static SignedLedger ReadZippedLedger(byte[] data)
        {
            if (data == null)
                return null;

            using (var stream = new ByteStream(Zipper.Unzip(data)))
            {
                return stream.ReadSignedLedger();
            }
        }
    }
}