using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Utilities
{
    public class OSInfo
    {
        public static bool TryGetWindowsVersion(out System.Version osVersion)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var osVersionString = Regex.Match(RuntimeInformation.OSDescription.Trim(), @"(?<=\s)((?>\d+.){1,3}(?>\d+)(?>\/\d+)?)$").Value;
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
