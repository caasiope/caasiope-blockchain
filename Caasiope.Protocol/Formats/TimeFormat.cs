using System;
using Caasiope.NBitcoin;

namespace Caasiope.Protocol.Formats
{
    public static class TimeFormat
    {
        public static DateTime ToDateTime(long timestamp)
        {
            return new DateTime(1970, 1, 1).AddSeconds(timestamp);
        }

        public static long ToTimetamp(DateTime datetime)
        {
            return datetime.ToUnixTimestamp();
        }
    }
}
