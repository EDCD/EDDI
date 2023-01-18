using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Utilities
{
    public static class CustomExtensionMethods
    {
        #region JTokenExtensions

        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }

        #endregion
        
        #region ObjectExtensions

        public static bool DeepEquals(this object obj, object another)
        {
            if (ReferenceEquals(obj, another)) { return true; }
            if ((obj == null) || (another == null)) { return false; }
            if (obj.GetType() != another.GetType()) { return false; }

            var objJson = JsonConvert.SerializeObject(obj);
            var anotherJson = JsonConvert.SerializeObject(another);

            return objJson == anotherJson;
        }

        #endregion

        #region StringExtensions

        // Remove undesired characters from end of a string (like the +++ on the end of some station names)
        public static string ReplaceEnd(this string str, char endreplace)
        {
            if (string.IsNullOrEmpty(str) || !char.IsWhiteSpace(endreplace)) { return str; }
            str = str.Trim();
            var ep = str.Length - 1;
            while (ep >= 0 && str[ep] == endreplace)
            {
                ep--;
            }
            return str.Substring(0, ep + 1);
        }

        public static string ToInvariantString(this decimal? dec)
        {
            return dec?.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this decimal dec)
        {
            return dec.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        #endregion
    }
}
