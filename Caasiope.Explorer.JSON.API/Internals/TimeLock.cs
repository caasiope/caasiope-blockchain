using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class TimeLock : TxAddressDeclaration
    {
        public long Timestamp;

        public TimeLock(long timestamp, string address) : this()
        {
            Timestamp = timestamp;
            Address = address;
        }

        public TimeLock() : base((byte)DeclarationType.TimeLock) { }
    }
}