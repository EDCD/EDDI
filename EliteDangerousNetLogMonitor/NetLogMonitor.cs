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

        private static void HandleNetLogLine(string line, Action<dynamic> callback)
        {
            Logging.Debug("Looking at line " + line);
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

                JObject result = new JObject();
                result["type"] = "Location";
                result["starsystem"] = oldMatch.Groups[3].Value;
                result["environment"] = oldMatch.Groups[4].Value;
                callback(result);
            }

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

                dynamic result = new JObject();
                result.type = "Location";
                result.starsystem = match.Groups[2].Value;
                result.x = match.Groups[3].Value;
                result.y = match.Groups[4].Value;
                result.z = match.Groups[5].Value;
                result.environment = match.Groups[6].Value;
                Logging.Debug("Callback with " + result);
                callback(result);
            }
        }
    }
}
