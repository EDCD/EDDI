using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EddiSpeechService
{
    public partial class Translations
    {
        /// <summary>Fix up body names</summary>
        private static string getPhoneticBody(string body, bool useICAO = false)
        {
            if (body == null) { return null; }

            // Use a regex to break apart the body from the system
            var shortBody = SHORTBODY.Match(body);

            if (!shortBody.Success)
            {
                // There was no match so we pass this as-is
                return body;
            }

            var results = new List<string>();

            // Extract and parse any leading star system name first
            var system = body.Substring(0, body.Length - shortBody.Groups[0].Value.Length).Trim();
            if (!string.IsNullOrEmpty(system))
            {
                results.Add(getPhoneticStarSystem(system, useICAO));
            }

            // Parse the short body name
            for (int i = 1; i < shortBody.Groups.Count; i++)
            {
                for (int j = 0; j < shortBody.Groups[i].Captures.Count; j++)
                {
                    var part = shortBody.Groups[i].Captures[j].Value.Trim();
                    var lastPart = GetLastPart(shortBody, i, j);
                    var nextPart = GetNextPart(shortBody, i, j);

                    if (string.IsNullOrEmpty(part)) { continue; }
                    if (part == "Belt" || part == "Cluster" || part == "Ring")
                    {
                        // Pass as-is
                        results.Add(part);
                    }
                    else if (DIGIT.IsMatch(part) || MOON.IsMatch(part) || nextPart == "Ring" || nextPart == "Belt" || lastPart == "Cluster")
                    {
                        // The part represents a body, possibly part of the name of a moon, ring, (stellar) belt, or belt cluster; 
                        // e.g. "Pru Aescs NC-M d7-192 A A Belt", "Prai Flyou JQ-F b30-3 B Belt Cluster 9", "Oopailks NV-X c17-1 AB 6 A Ring"
                        results.Add(sayAsLettersOrNumbers(part, true, useICAO));
                    }
                    else if (SUBSTARS.IsMatch(part))
                    {
                        // The part is uppercase; turn it in to ICAO if required
                        results.Add(sayAsLettersOrNumbers(part, false, useICAO));
                    }

                    else if (TEXT.IsMatch(part))
                    {
                        results.Add(sayAsLettersOrNumbers(part, true, useICAO));
                    }
                    else
                    {
                        // Pass it as-is
                        results.Add(part);
                    }
                }
            }

            return Regex.Replace(string.Join(" ", results), @"\s+", " ");
        }

        private static string GetLastPart(Match match, int i, int j)
        {
            if (j > 0)
            {
                return match.Groups[i].Captures[j - 1].Value.Trim();
            }
            if (i > 2 && match.Groups[i - 1].Captures.Count > 0)
            {
                return match.Groups[i - 1].Captures[match.Groups[i - 1].Captures.Count - 1].Value.Trim();
            }
            return null;
        }

        private static string GetNextPart(Match match, int i, int j)
        {
            if (j < match.Groups[i].Captures.Count - 1)
            {
                return match.Groups[i].Captures[j + 1].Value.Trim();
            }
            if (i < match.Groups.Count - 1 && match.Groups[i + 1].Captures.Count > 0)
            {
                return match.Groups[i + 1].Captures[0].Value.Trim();
            }
            return null;
        }
    }
}
