using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Utilities
{
    public abstract class Redaction
    {
        // These are case sensitive property names.
        // Strip module data that is not useful to report for more consistent matching
        // Strip commodity data that is not useful to report for more consistent matching
        // Strip sensitive or personal data like "apiKey" or "frontierID"
        private static readonly string[] PersonalProperties =
        {
            "on",
            "priority",
            "health",
            "buyprice",
            "stock",
            "stockbracket",
            "sellprice",
            "demand",
            "demandbracket",
            "StatusFlags",
            "apiKey",
            "frontierID",
            "FID",
            "ActiveFine",
            "CockpitBreach",
            "BoostUsed",
            "FuelLevel",
            "FuelUsed",
            "JumpDist",
            "Wanted",
            "Latitude",
            "Longitude",
            "MyReputation",
            "SquadronFaction",
            "HappiestSystem",
            "HomeSystem",
            "access_token",
            "refresh_token",
            "uploaderID",
            "commanderName"
        };

        // The order here is important: we should redact the most specific strings first
        private static readonly string[] envVarsToRedact =
        {
            "TEMP",
            "TMP",
            "APPDATA",
            "LOCALAPPDATA",
            "USERPROFILE",
            "HOMEPATH",
            "USERNAME",
        };

        public static JToken RedactPersonalProperties ( JToken data )
        {
            if ( data is JObject objectData )
            {
                foreach ( string property in PersonalProperties )
                {
                    objectData.Descendants()
                        .OfType<JProperty>()
                        .Where( attr => attr.Name.StartsWith( property, StringComparison.OrdinalIgnoreCase ) )
                        .ToList()
                        .ForEach( attr => attr.Remove() );
                }
                return objectData;
            }
            if ( data is JArray arrayData )
            {
                arrayData.Descendants()
                    .OfType<JProperty>()
                    .ToList()
                    .ForEach( attr => RedactPersonalProperties( attr.Value ) );
                return arrayData;
            }
            return data;
        }

        /// <summary>Removes potentially personally identifying data by replacing expanded environment variables with their percent-quoted names.</summary>
        public static string RedactEnvironmentVariables ( string rawString )
        {
            var result = envVarsToRedact.Aggregate(rawString, RedactEnvironmentVariable);
            return result;
        }

        public static JToken RedactEnvironmentVariables ( JToken data )
        {
            if ( data.Type is JTokenType.String )
            {
                RedactEnvironmentVariables( data.ToString() );
            }
            else if ( data is JObject objectData )
            {
                objectData.Descendants()
                    .OfType<JProperty>()
                    .Where( attr => attr.Value.Type is JTokenType.String )
                    .ToList()
                    .ForEach( attr => RedactEnvironmentVariables( attr.Value ) );
                return objectData;
            }
            else if ( data is JArray arrayData )
            {
                arrayData.Descendants()
                    .OfType<JProperty>()
                    .ToList()
                    .ForEach( attr => RedactEnvironmentVariables( attr.Value ) );
                return arrayData;
            }
            return data;
        }

        private static string RedactEnvironmentVariable ( string rawString, string envVar )
        {
            if ( string.IsNullOrEmpty( rawString ) )
            {
                return rawString;
            }

            var envVarExpansion = Environment.GetEnvironmentVariable( envVar );
            if ( string.IsNullOrEmpty( envVarExpansion ) || envVarExpansion == @"\" )
            {
                return rawString;
            }

            var redacted = rawString.Replace( envVarExpansion, $"%{envVar}%" );
            return redacted;
        }
    }
}
