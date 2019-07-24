﻿using System;
using System.Text.RegularExpressions;

namespace Utilities
{
    public struct Version : IEquatable<Version>, IComparable<Version>
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

        // this is the most efficient constructor as it doesn't need to do any parsing
        public Version(int major, int minor, int patch, TestPhase phase = TestPhase.final, int iteration = 0)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.phase = phase;
            this.iteration = iteration;
        }

        // can throw ArgumentException for an invalid phase
        public Version(int major, int minor, int patch, string phase, int iteration)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.phase = (TestPhase)Enum.Parse(typeof(TestPhase), phase);
            this.iteration = iteration;
        }

        // Can throw FormatException or ArgumentException
        public Version(string input)
        {
            // Performance note: after considering 
            // https://docs.microsoft.com/en-us/dotnet/standard/base-types/best-practices?view=netframework-4.7.2 
            // I have concluded that an interpreted rather than compiled regex is the better choice here,
            // as version strings are parsed infrequently.
            string pattern = @"^(?<major>\d+).(?<minor>\d+).(?<patch>\d+)(-(?<phase>[a-z]+)(?<iteration>\d+))?$";
            Match match = Regex.Match(input, pattern);
            GroupCollection matchGroups = match.Groups;

            major = int.Parse(matchGroups["major"].Value);
            minor = int.Parse(matchGroups["minor"].Value);
            patch = int.Parse(matchGroups["patch"].Value);
            string phaseStr = matchGroups["phase"].Value;

            // special handling for final version strings
            if (String.IsNullOrEmpty(phaseStr))
            {
                phase = TestPhase.final;
                iteration = 0;
            }
            else
            {
                phase = (TestPhase)Enum.Parse(typeof(TestPhase), phaseStr);
                iteration = int.Parse(matchGroups["iteration"].Value);
            }
        }

        public override string ToString()
        {
            return phase == TestPhase.final
                ? $"{major}.{minor}.{patch}"
                : $"{major}.{minor}.{patch}-{phase}{iteration}";
        }

        public string ShortString => $"{major}.{minor}.{patch}";

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
            // this is not critical as we don't use collections of Versions, so just do something fast that mixes in all fields, multiplying by a prime number
            int hashCode = 17;
            hashCode = hashCode * 23 + major;
            hashCode = hashCode * 23 + minor;
            hashCode = hashCode * 23 + patch;
            hashCode = hashCode * 23 + (int)phase;
            hashCode = hashCode * 23 + iteration;
            return hashCode;
        }

        public int CompareTo(Version other)
        {
            if (major > other.major)
            {
                return 1;
            }
            if (major < other.major)
            {
                return -1;
            }

            if (minor > other.minor)
            {
                return 1;
            }
            if (minor < other.minor)
            {
                return -1;
            }

            if (patch > other.patch)
            {
                return 1;
            }
            if (patch < other.patch)
            {
                return -1;
            }

            if (phase > other.phase)
            {
                return 1;
            }
            if (phase < other.phase)
            {
                return -1;
            }

            if (iteration > other.iteration)
            {
                return 1;
            }
            if (iteration < other.iteration)
            {
                return -1;
            }

            return 0;
        }

        public static bool operator ==(Version left, Version right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Version left, Version right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Version left, Version right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Version left, Version right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Version left, Version right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Version left, Version right)
        {
            return left.CompareTo(right) >= 0;
        }

        // >0 if the first version is greater than the second version, 0 if they are the same, <0 if first version is less than the second version
        public static int CompareStrings(string Version1, string Version2)
        {
            Version v1 = new Version(Version1);
            Version v2 = new Version(Version2);
            return v1.CompareTo(v2);
        }
    }
}
