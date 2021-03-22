using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EddiSpeechService
{
    public partial class Translations
    {
        /// <summary>Fix up body names</summary>
        private static string getPhoneticBody(string body, bool useICAO = false)
        {
            if (body == null)
            {
                return null;
            }

            List<string> results = new List<string>();

            // Use a regex to break apart the body from the system
            Match match = BODY.Match(body);
            if (!match.Success)
            {
                // There was no match so we pass this as-is
                return body;
            }
            else
            {
                // Might be a short body name
                if (SHORTBODY.IsMatch(body))
                {
                    return sayAsLettersOrNumbers(body, true, useICAO);
                }

                // Parse the starsystem
                results.Add(getPhoneticStarSystem(match.Groups[1].Value.Trim(), useICAO));
                // Parse the body
                for (int i = 2; i < match.Groups.Count; i++)
                {
                    for (int j = 0; j < match.Groups[i].Captures.Count; j++)
                    {
                        var part = match.Groups[i].Captures[j].Value.Trim();
                        var lastPart = j > 0 ? match.Groups[i].Captures[j - 1].Value.Trim()
                            : i > 2 && match.Groups[i - 1].Captures.Count > 0 ? match.Groups[i - 1].Captures[match.Groups[i - 1].Captures.Count - 1].Value.Trim()
                            : null;
                        var nextPart = j < match.Groups[i].Captures.Count - 1 ? match.Groups[i].Captures[j + 1].Value.Trim()
                            : i < match.Groups.Count - 1 ? match.Groups[i + 1].Captures[0].Value.Trim()
                            : null;

                        if (DIGIT.IsMatch(part))
                        {
                            // The part is a number; turn it in to ICAO if required
                            results.Add(useICAO ? ICAO(part, true) : part);
                        }
                        else if (PLANET.IsMatch(part) || lastPart == "Cluster" || nextPart == "Ring" || nextPart == "Belt")
                        {
                            // The part represents a body, possibly part of the name of a moon, ring, (stellar) belt, or belt cluster; 
                            // e.g. "Pru Aescs NC-M d7-192 A A Belt", "Prai Flyou JQ-F b30-3 B Belt Cluster 9", "Oopailks NV-X c17-1 AB 6 A Ring"

                            // turn it in to ICAO if required
                            if (useICAO)
                            {
                                results.Add(ICAO(part, true));
                            }
                            else
                            {
                                results.Add(sayAsLettersOrNumbers(part, true, useICAO));
                            }
                        }
                        else if (part == "Belt" || part == "Cluster" || part == "Ring")
                        {
                            // Pass as-is
                            results.Add(part);
                        }
                        else if (SUBSTARS.IsMatch(part))
                        {
                            // The part is uppercase; turn it in to ICAO if required
                            results.Add(UPPERCASE.Replace(part, m => useICAO ? ICAO(m.Value, true) : string.Join<char>(" ", m.Value)));
                        }
                        else if (TEXT.IsMatch(part))
                        {
                            // turn it in to ICAO if required
                            if (useICAO)
                            {
                                results.Add(ICAO(part));
                            }
                            else
                            {
                                results.Add(sayAsLettersOrNumbers(part, true, useICAO));
                            }
                        }
                        else
                        {
                            // Pass it as-is
                            results.Add(part);
                        }
                    }
                }
            }

            return Regex.Replace(string.Join(" ", results), @"\s+", " ");
        }
    }
}
