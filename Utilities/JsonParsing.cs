using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Utilities
{
    public class JsonParsing
    {
        /// <summary>
        /// Provide helper functions for parsing json files
        /// </summary>

        public static DateTime getDateTime(string key, IDictionary<string, object> data)
        {
            data.TryGetValue(key, out object val);
            return getDateTime(key, val);
        }

        public static DateTime getDateTime(string key, JObject data)
        {
            data.TryGetValue(key, out JToken val);
            return getDateTime(key, val);
        }

        public static DateTime getDateTime(string key, object val)
        {
            // DateTime.Parse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
            // and (DateTime.Parse(timestamp).ToUniversalTime() are equivalent and both return a TimeStamp in UTC.
            if (val == null)
            {
                throw new ArgumentNullException("Expected value for " + key + " not present");
            }
            if (val is DateTime dtime)
            {
                return dtime.ToUniversalTime();
            }
            if (val is JToken jToken)
            {
                return getDateTime(key, jToken.ToString());
            }
            if (val is string str)
            {
                if (DateTime.TryParseExact(str, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
                {
                    // Journal format ("2019-09-24T02:40:34Z")
                    return result.ToUniversalTime();
                }
                if (DateTime.TryParseExact(str, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result))
                {
                    // EDSM format ("2018-03-28 22:12:20")
                    return result.ToUniversalTime();
                }
                return DateTime.Parse(str).ToUniversalTime();
            }
            throw new ArgumentException("Unparseable value for " + key);
        }

        public static decimal getDecimal(IDictionary<string, object> data, string key)
        {
            data.TryGetValue(key, out object val);
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
            data.TryGetValue(key, out object val);
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
            data.TryGetValue(key, out object val);
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
            data.TryGetValue(key, out object val);
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
            data.TryGetValue(key, out object val);
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
            data.TryGetValue(key, out object val);
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
            data.TryGetValue(key, out object val);
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
            if (data.TryGetValue(key, out object val))
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
            data.TryGetValue(key, out object val);
            return (string)val;
        }

        public static bool compareJsonEquality<T>(T self, T to, bool jsonIgnore, out string mutatedPropertyName, string[] ignore = null) where T : class
        {
            mutatedPropertyName = string.Empty;
            if (self != null && to != null)
            {
                Type type = typeof(T);
                List<string> ignoreList = new List<string>(ignore);
                foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (jsonIgnore)
                    {
                        var attributes = pi.GetCustomAttributes(true);
                        foreach (var attribute in attributes)
                        {
                            if (attribute is JsonIgnoreAttribute)
                            {
                                ignoreList.Add(pi.Name);
                            }
                        }
                    }

                    if (!ignoreList.Contains(pi.Name))
                    {
                        object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                        object toValue = type.GetProperty(pi.Name).GetValue(to, null);

                        if (selfValue != toValue && (selfValue == null || !selfValue.Equals(toValue)))
                        {
                            if (selfValue is object || selfValue is object[] || selfValue is List<object>)
                            {
                                if (compareJsonEquality(selfValue, toValue, jsonIgnore, out mutatedPropertyName, ignore))
                                {
                                    continue;
                                }
                            }
                            mutatedPropertyName = pi.Name;
                            return false;
                        }
                    }
                }
                return true;
            }
            return self == to;
        }
    }
}
