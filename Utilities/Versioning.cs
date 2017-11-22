using System;

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
            System.Text.RegularExpressions.Regex versionRegex = new System.Text.RegularExpressions.Regex(@"^([\d]+)\.([\d]+)\.([\d]+)-?([a-z]+)?([\d]+)?");
            System.Text.RegularExpressions.MatchCollection m1 = versionRegex.Matches(Version1);
            System.Text.RegularExpressions.MatchCollection m2 = versionRegex.Matches(Version2);

            // Handle simple version number differences
            for (int i = 1; i < 4; i++)
            {
                if (Convert.ToInt32(m1[0].Groups[i].Value) > Convert.ToInt32(m2[0].Groups[i].Value))
                {
                    return 1;
                }
                if (Convert.ToInt32(m1[0].Groups[i].Value) < Convert.ToInt32(m2[0].Groups[i].Value))
                {
                    return -1;
                }
            }

            // Handle situation where one is pre-release and the other is not
            if (!m1[0].Groups[4].Success && m2[0].Groups[4].Success)
            {
                return 1;
            }
            if (m1[0].Groups[4].Success && !m2[0].Groups[4].Success)
            {
                return -1;
            }
            if (!m1[0].Groups[4].Success && !m2[0].Groups[4].Success)
            {
                // No pre-release so done
                return 0;
            }

            // Handle situation where one is alpha and the other beta
            if (m1[0].Groups[4].Value[0] > m2[0].Groups[4].Value[0])
            {
                return 1;
            }
            if (m1[0].Groups[4].Value[0] < m2[0].Groups[4].Value[0])
            {
                return -1;
            }

            // Handle situation with different prerelease numbers
            if (Convert.ToInt32(m1[0].Groups[5].Value) > Convert.ToInt32(m2[0].Groups[5].Value))
            {
                return 1;
            }
            if (Convert.ToInt32(m1[0].Groups[5].Value) < Convert.ToInt32(m2[0].Groups[5].Value))
            {
                return -1;
            }

            return 0;
        }
    }
}
