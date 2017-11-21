using System;

namespace Utilities
{
    public class Dates
    {
        /// <summary>
        /// Provide a DateTime given a unix timestamp in seconds
        /// </summary>
        public static DateTime? fromTimestamp(long? timestamp)
        {
            return timestamp == null ? (DateTime?)null : new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToInt64(timestamp));
        }
    }
}
