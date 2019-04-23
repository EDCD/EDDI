using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public class Redaction
    {
        /// <summary>Removes potentially personally identifying data by replacing expanded environment variables with their percent-quoted names.</summary>
        public static string RedactEnvironmentVariables(string rawString)
        {
            // The order here is important: we should redact the most specific strings first
            List<string> envVarsToRedact = new List<string>()
            {
                "TEMP",
                "TMP",
                "APPDATA",
                "LOCALAPPDATA",
                "USERPROFILE",
                "HOMEPATH",
                "USERNAME",
            };
            string result = envVarsToRedact.Aggregate(rawString, RedactEnvironmentVariable);
            return result;
        }

        internal static string RedactEnvironmentVariable(string rawString, string envVar)
        {
            if (String.IsNullOrEmpty(rawString))
            {
                return rawString;
            }
            string envVarExpansion = Environment.GetEnvironmentVariable(envVar);
            if (String.IsNullOrEmpty(envVarExpansion) || envVarExpansion == @"\" )
            {
                return rawString;
            }
            string redacted = rawString.Replace(envVarExpansion, $"%{envVar}%");
            return redacted;
        }
    }
}
