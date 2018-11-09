using System.Diagnostics;

namespace Caasiope.Protocol.Types
{
    public class SignatureByte
    {
        // TODO maybe use uint256
        public readonly byte[] Bytes;
        public const int SIZE = 65;

        public SignatureByte(byte[] bytes)
        {
            Debug.Assert(bytes.Length == SIZE);
            Bytes = bytes;
        }
    }
}