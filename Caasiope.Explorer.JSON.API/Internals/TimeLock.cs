using Caasiope.Protocol.Types;

namespace Caasiope.Explorer.JSON.API.Internals
{
    public class TimeLock : TxDeclaration
    {
        public long Timestamp;

        public TimeLock(long timestamp) : this()
        {
            Timestamp = timestamp;
        }

        public TimeLock() : base((byte)DeclarationType.TimeLock) { }
    }
}