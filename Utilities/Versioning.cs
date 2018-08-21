using System;

namespace Utilities
{
    public struct Version
    {
        public enum TestPhase
        {
            a = -3, // alpha
            b = -2, // beta
            rc = -1, // release candidate
            final = 0, // final
        }

        public readonly int major;
        public readonly int minor;
        public readonly int patch;
        public readonly TestPhase phase; // not printed for final
        public readonly int iteration; // not printed for final

        public Version(int major, int minor, int patch, TestPhase phase = TestPhase.final, int iteration = 0)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.phase = phase;
            this.iteration = iteration;
        }

        public override string ToString()
        {
            return phase == TestPhase.final 
                ? $"{major}.{minor}.{patch}" 
                : $"{major}.{minor}.{patch}-{phase}{iteration}";
        }
    }

    public class Versioning
    {
        /// <summary>
        /// Compare two version strings
        /// </summary>
        /// <returns>1 if the first version is greater than the second version, 0 if they are the same, -1 if first version is less than the second version</returns>
        public static int Compare(string Version1, string Version2)
        {
            try
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
            catch (Exception ex)
            {
                Logging.Error("Version comparison failed. ", ex);
                throw;
            }
        }
    }
}
