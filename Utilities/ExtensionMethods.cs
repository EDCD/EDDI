using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace System
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> RemoveNulls<T> ( this IEnumerable<T> items )
        {
            return items.Where( i => i != null );
        }
    }

    public static class JTokenExtensions
    {
        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }

    public static partial class ObjectExtensions
    {
        public static bool DeepEquals(this object obj, object another)
        {
            if (ReferenceEquals(obj, another)) { return true; }
            if ((obj == null) || (another == null)) { return false; }
            if (obj.GetType() != another.GetType()) { return false; }

            try
            {
                return JsonConvert.SerializeObject( obj ) == JsonConvert.SerializeObject( another );
            }
            catch ( Exception e )
            {
                Logging.Error("Value equality comparison failed.", e);
            }

            return false;
        }
    }

    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        // Remove undesired characters from end of a string (like the +++ on the end of some station names)
        public static string ReplaceEnd(this string str, char endreplace)
        {
            if (string.IsNullOrEmpty(str) || char.IsWhiteSpace(endreplace)) { return str; }
            str = str.Trim();
            var ep = str.Length - 1;
            while (ep >= 0 && str[ep] == endreplace)
            {
                ep--;
            }
            return str.Substring(0, ep + 1).Trim();
        }

        public static string ToInvariantString(this decimal? dec)
        {
            return dec?.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string ToInvariantString(this decimal dec)
        {
            return dec.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Calculates the Levenshtein Distance between two strings (e.g. the number of edits required to transform one string into another). Lower values indicate more similarity. Typical usage is for creating an ordered list of values sorted by similarity to a target value.
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="targetString"></param>
        /// <param name="comparisonType"></param>
        /// <returns></returns>
        public static int LevenshteinDistance ( this string sourceString, string targetString, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase )
        {
            var sourceStringLength = sourceString.Length;
            var targetStringLength = targetString.Length;
            var arrayDistance = new int[sourceStringLength + 1, targetStringLength + 1];

            if ( sourceStringLength == 0 )
            {
                return targetStringLength;
            }
            if ( targetStringLength == 0 )
            {
                return sourceStringLength;
            }

            for ( var i = 0; i <= sourceStringLength; i++ )
            {
                arrayDistance[ i, 0 ] = i;
            }

            for ( var j = 0; j <= targetStringLength; j++ )
            {
                arrayDistance[ 0, j ] = j;
            }

            for ( var i = 1; i <= sourceStringLength; i++ )
            {
                for ( var j = 1; j <= targetStringLength; j++ )
                {
                    var cost = string.Equals( targetString[j - 1].ToString(), sourceString[i - 1].ToString(), comparisonType ) ? 0 : 1;
                    arrayDistance[ i, j ] = Math.Min( Math.Min( arrayDistance[ i - 1, j ] + 1, arrayDistance[ i, j - 1 ] + 1 ), arrayDistance[ i - 1, j - 1 ] + cost );
                }
            }

            return arrayDistance[ sourceStringLength, targetStringLength ];
        }
    }
}
