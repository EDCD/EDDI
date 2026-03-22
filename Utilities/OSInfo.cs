using System.Runtime.InteropServices;

namespace Utilities
{
    public static class OSInfo
    {
        public static bool TryGetWindowsVersion(out System.Version osVersion)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var osVersionString = GeneratedRegex.OsVersionRegex().Match(RuntimeInformation.OSDescription.Trim()).Value;
                if (!string.IsNullOrEmpty(osVersionString))
                {
                    if (System.Version.TryParse(osVersionString, out osVersion))
                    {
                        return true;
                    }
                }
            }
            osVersion = null;
            return false;
        }
    }
}
