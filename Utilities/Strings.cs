using System.Text;

namespace Utilities
{
    public static class Strings
    {
        /// <summary> Remove unacceptable characters from strings written to json</summary>
        public static string JSONify(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (c != '"' && c != '\\')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
