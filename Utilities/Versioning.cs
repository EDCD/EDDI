using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Versioning
    {
        /// <summary>
        /// Compare two version strings
        /// </summary>
        /// <returns>1 if the first version is greater than the second version</returns>
        public static int Compare(string Version1, string Version2)
        {
            if (Version1 == null && Version2 == null)
            {
                return 0;
            }
            if (Version1 != null && Version2 == null)
            {
                return 1;
            }
            if (Version1 == null && Version2 != null)
            {
                return -1;
            }
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"([\d]+)");
            System.Text.RegularExpressions.MatchCollection m1 = regex.Matches(Version1);
            System.Text.RegularExpressions.MatchCollection m2 = regex.Matches(Version2);
            int min = Math.Min(m1.Count, m2.Count);
            for (int i = 0; i < min; i++)
            {
                if (Convert.ToInt32(m1[i].Value) > Convert.ToInt32(m2[i].Value))
                {
                    return 1;
                }
                if (Convert.ToInt32(m1[i].Value) < Convert.ToInt32(m2[i].Value))
                {
                    return -1;
                }
            }
            int max = Math.Max(m1.Count, m2.Count);
            for (int i = min; i < max; i++)
            {
                int v1 = (m1.Count < i ? Convert.ToInt32(m1[i].Value) : 0);
                int v2 = (m2.Count < i ? Convert.ToInt32(m2[i].Value) : 0);
                if (v1 > v2)
                {
                    return 1;
                }
                if (v1 < v2)
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}
