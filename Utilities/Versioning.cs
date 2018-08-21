using System;
using System.Text.RegularExpressions;

namespace Utilities
{
    public struct Version : IEquatable<Version>
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

        // can throw ArgumentException for an invalid phase
        public Version(int major, int minor, int patch, string phase, int iteration)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.phase = (TestPhase)Enum.Parse(typeof(TestPhase), phase);
            this.iteration = iteration;
        }

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

        // Can throw FormatException or ArgumentException
        public static Version Parse(string input)
        {
            // Performance note: after considering 
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/best-practices?view=netframework-4.7.2 
            // I have concluded that an interpreted rather than compiled regex is the better choice here,
            // as version strings are parsed infrequently.
            string pattern = @"^(?<major>\d+).(?<minor>\d+).(?<patch>\d+)(-(?<phase>[a-z]+)(?<iteration>\d+))?$";
            Match match = Regex.Match(input, pattern);
            GroupCollection matchGroups = match.Groups;

            int major = int.Parse(matchGroups["major"].Value);
            int minor = int.Parse(matchGroups["minor"].Value);
            int patch = int.Parse(matchGroups["patch"].Value);
            string phase = matchGroups["phase"].Value;
            int iteration = 0;

            // special handling for final version strings
            if (String.IsNullOrEmpty(phase))
            {
                phase = TestPhase.final.ToString();
            }
            else
            {
                iteration = int.Parse(matchGroups["iteration"].Value);
            }

            return new Version(major, minor, patch, phase, iteration);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Version))
            {
                return false;
            }

            Version version = (Version)obj;
            return this.Equals(version);
        }

        public bool Equals(Version version)
        {
            return major == version.major &&
                   minor == version.minor &&
                   patch == version.patch &&
                   phase == version.phase &&
                   iteration == version.iteration;
        }

        public override int GetHashCode()
        {
            var hashCode = -458428195;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + major.GetHashCode();
            hashCode = hashCode * -1521134295 + minor.GetHashCode();
            hashCode = hashCode * -1521134295 + patch.GetHashCode();
            hashCode = hashCode * -1521134295 + phase.GetHashCode();
            hashCode = hashCode * -1521134295 + iteration.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Version left, Version right)
        {
            return
                left.major == right.major &&
                left.minor == right.minor &&
                left.patch == right.patch &&
                left.phase == right.phase &&
                left.iteration == right.iteration;
        }

        public static bool operator !=(Version left, Version right)
        {
            return !left.Equals(right);
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
                Regex versionRegex = new Regex(@"^([\d]+)\.([\d]+)\.([\d]+)-?([a-z]+)?([\d]+)?");
                MatchCollection m1 = versionRegex.Matches(Version1);
                MatchCollection m2 = versionRegex.Matches(Version2);

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
