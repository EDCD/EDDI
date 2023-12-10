using System;
using System.Collections.Generic;
using System.Linq;

namespace Utilities
{
    public abstract class Redaction
    {
        /// <summary>Removes potentially personally identifying data by replacing expanded environment variables with their percent-quoted names.</summary>
        public static string RedactEnvironmentVariables ( string rawString )
        {
            // The order here is important: we should redact the most specific strings first
            var envVarsToRedact = new List<string>()
            {
                "TEMP",
                "TMP",
                "APPDATA",
                "LOCALAPPDATA",
                "USERPROFILE",
                "HOMEPATH",
                "USERNAME",
            };
            var result = envVarsToRedact.Aggregate(rawString, RedactEnvironmentVariable);
            return result;
        }

        private static string RedactEnvironmentVariable ( string rawString, string envVar )
        {
            if ( string.IsNullOrEmpty( rawString ) )
            {
                return rawString;
            }
            var envVarExpansion = Environment.GetEnvironmentVariable(envVar);
            if ( string.IsNullOrEmpty( envVarExpansion ) || envVarExpansion == @"\" )
            {
                return rawString;
            }
            var redacted = rawString.Replace(envVarExpansion, $"%{envVar}%");
            return redacted;
        }
    }
}
