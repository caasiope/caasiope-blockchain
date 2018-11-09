namespace Caasiope.Protocol.Types
{
    public class TimeLockAccount
    {
        public readonly Address Address;
        public readonly long Timestamp;

        public TimeLockAccount(Address address, long timestamp)
        {
            Address = address;
            Timestamp = timestamp;
        }
    }
}