using System;

namespace Utilities
{
    public class Dates
    {
        /// <summary> Provide a DateTime given a unix timestamp in seconds </summary>
        public static DateTime? fromTimestamp(long? timestamp)
        {
            return timestamp == null ? (DateTime?)null : new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToInt64(timestamp));
        }

        /// <summary> Provide a unix timestamp in seconds given a DateTime </summary>
        public static long? fromDateTimeStringToSeconds(string dateTime)
        {
            return dateTime == null ? null : (long?)(DateTime.Parse(dateTime).Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;
        }
    }
}
