using System;

namespace Utilities
{
    public class Redaction
    {
        public static string RedactEnvironmentVariables(string rawString)
        {
            return RedactEnvironmentVariable(rawString, "APPDATA");
        }

        internal static string RedactEnvironmentVariable(string rawString, string envVar)
        {
            string envVarExpansion = Environment.GetEnvironmentVariable(envVar);
            string redacted = rawString.Replace(envVarExpansion, $"%{envVar}%");
            return redacted;
        }
    }
}
