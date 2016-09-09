using EliteDangerousEvents;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Utilities;

namespace EliteDangerousNetLogMonitor
{
    /// <summary>A log monitor for the Elite: Dangerous netlog</summary>
    public class NetLogMonitor : LogMonitor
    {
        public NetLogMonitor(NetLogConfiguration configuration, Action<dynamic> callback) : base(configuration.path, @"^netLog\.[0-9\.]+\.log$", (result) => HandleNetLogLine(result, callback))
        {
        }

        private static Regex OldSystemRegex = new Regex(@"^{([0-9][0-9]:[0-9][0-9]:[0-9][0-9])} System:([0-9]+)\(([^\)]+)\).* ([A-Za-z]+)$");
        // {19:24:56} System:"Wolf 397" StarPos:(40.000,79.219,-10.406)ly Body:23 RelPos:(-2.01138,1.32957,1.7851)km NormalFlight
        private static Regex SystemRegex = new Regex(@"^{([0-9][0-9]:[0-9][0-9]:[0-9][0-9])} System:""([^""]+)"" StarPos:\((-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+),(-?[0-9]+\.[0-9]+)\)ly .*? ([A-Za-z]+)$");

        private static string lastStarsystem = null;
        private static string lastEnvironment = null;

        private static void HandleNetLogLine(string line, Action<dynamic> callback)
        {
            string starSystem = null;
            string environment = null;
            decimal x = 0;
            decimal y = 0;
            decimal z = 0;

            Match oldMatch = OldSystemRegex.Match(line);
            if (oldMatch.Success)
            {
                Logging.Debug("Match against old regex");
                if (@"Training" == oldMatch.Groups[3].Value || @"Destination" == oldMatch.Groups[3].Value)
                {
                    // We ignore training missions
                    return;
                }

                if (@"ProvingGround" == oldMatch.Groups[4].Value)
                {
                    // We ignore CQC
                    return;
                }

                starSystem = oldMatch.Groups[3].Value;
                environment = oldMatch.Groups[3].Value;
            }
            else
            {
                Match match = SystemRegex.Match(line);
                if (match.Success)
                {
                    Logging.Debug("Match against new regex");
                    if (@"Training" == match.Groups[2].Value || @"Destination" == match.Groups[2].Value)
                    {
                        // We ignore training missions
                        return;
                    }

                    if (@"ProvingGround" == match.Groups[6].Value)
                    {
                        // We ignore CQC
                        return;
                    }

                    starSystem = match.Groups[2].Value;
                    environment = match.Groups[6].Value;
                    x = Math.Round(decimal.Parse(match.Groups[3].Value) * 32) / (decimal)32.0;
                    y = Math.Round(decimal.Parse(match.Groups[4].Value) * 32) / (decimal)32.0;
                    z = Math.Round(decimal.Parse(match.Groups[5].Value) * 32) / (decimal)32.0;
                }
            }

            Event theEvent = null;
            if (starSystem != null)
            {
                if (starSystem != lastStarsystem)
                {
                    // Change of system
                    theEvent = new JumpedEvent(DateTime.Now, starSystem, x, y, z);
                    lastStarsystem = starSystem;
                    lastEnvironment = environment;
                }
                else if (environment != lastEnvironment)
                {
                    // Change of environment
                    if (environment == "Supercruise")
                    {
                        theEvent = new EnteredSupercruiseEvent(DateTime.Now);
                        lastEnvironment = environment;
                    }
                    else if (environment == "NormalFlight")
                    {
                        theEvent = new EnteredNormalSpaceEvent(DateTime.Now, starSystem);
                        lastEnvironment = environment;
                    }
                }
            }
            if (theEvent != null)
            {
                Logging.Debug("Returning event " + theEvent);
                callback(theEvent);
            }
            else
            {
                Logging.Debug("Could not resolve event");
            }
        }
    }
}
