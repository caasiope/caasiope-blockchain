namespace Caasiope.Explorer.JSON.API.Internals
{
    public class TimeLock : TxDeclaration
    {
        public readonly long Timestamp;

        public TimeLock(long timestamp)
        {
            Timestamp = timestamp;
        }
    }
}