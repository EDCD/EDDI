using System;
using System.Collections.Generic;

namespace Utilities
{
    public class JsonParsing
    {
        /// <summary>
        /// Provide helper functions for parsing json files
        /// </summary>

        public static decimal getDecimal(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getDecimal(key, val);
        }

        public static decimal getDecimal(string key, object val)
        {
            if (val == null)
            {
                throw new ArgumentNullException("Expected value for " + key + " not present");
            }
            if (val is long)
            {
                return (long)val;
            }
            else if (val is double)
            {
                return (decimal)(double)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        public static decimal? getOptionalDecimal(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getOptionalDecimal(key, val);
        }

        public static decimal? getOptionalDecimal(string key, object val)
        {
            if (val == null)
            {
                return null;
            }
            else if (val is long)
            {
                return (long?)val;
            }
            else if (val is double)
            {
                return (decimal?)(double?)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        public static int getInt(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getInt(key, val);
        }

        public static int getInt(string key, object val)
        {
            if (val is long)
            {
                return (int)(long)val;
            }
            else if (val is int)
            {
                return (int)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        public static int? getOptionalInt(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getOptionalInt(key, val);
        }

        public static int? getOptionalInt(string key, object val)
        {
            if (val == null)
            {
                return null;
            }
            else if (val is long)
            {
                return (int?)(long?)val;
            }
            else if (val is int)
            {
                return (int?)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        public static long getLong(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getLong(key, val);
        }

        public static long getLong(string key, object val)
        {
            if (val is long)
            {
                return (long)val;
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        public static long? getOptionalLong(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            if (val == null)
            {
                return null;
            }
            else if (val is long)
            {
                return (long?)val;
            }

            throw new ArgumentException($"Expected value of type long for key {key}, instead got value of type {data.GetType().FullName}");
        }

        public static bool getBool(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return getBool(key, val);
        }

        public static bool getBool(string key, object val)
        {
            if (val == null)
            {
                throw new ArgumentNullException("Expected value for " + key + " not present");
            }
            return (bool)val;
        }

        public static bool? getOptionalBool(IDictionary<string, object> data, string key)
        {
            object val;
            if (data.TryGetValue(key, out val))
            {
                return val as bool?;
            }
            else
            {
                return null;
            }
        }

        public static string getString(IDictionary<string, object> data, string key)
        {
            object val;
            data.TryGetValue(key, out val);
            return (string)val;
        }
    }
}
