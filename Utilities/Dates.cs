﻿using System;
using System.Globalization;

namespace Utilities
{
    public class Dates
    {
        /// <summary> Provide a DateTime given a unix timestamp in seconds </summary>
        public static DateTime? fromTimestamp(long? timestamp)
        {
            return timestamp == null ? (DateTime?)null : new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToInt64(timestamp));
        }

        /// <summary> Provide a unix timestamp in seconds given a DateTime string</summary>
        public static long? fromDateTimeStringToSeconds(string dateTime)
        {
            return dateTime == null ? null : (long?)(DateTime.Parse(dateTime).Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;
        }

        /// <summary> Provide a unix timestamp in seconds given a DateTime struct</summary>
        public static long fromDateTimeToSeconds(DateTime dateTime)
        {
            return (long)(dateTime.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;
        }

        public static string FromDateTimeToString(DateTime? dateTime)
        {
            return dateTime?.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture) ?? string.Empty;
        }

        public static DateTime? FromString(string dateTime)
        {
            if (!string.IsNullOrEmpty(dateTime) && 
                DateTime.TryParseExact(dateTime, "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal, out var date))
            {
                return date;
            }
            return null;
        }
    }
}
