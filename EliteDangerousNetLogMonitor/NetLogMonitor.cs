using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace EliteDangerousNetLogMonitor
{
    /// <summary>A log monitor for the Elite: Dangerous netlog</summary>
    public class NetLogMonitor : LogMonitor
    {
        public NetLogMonitor(NetLogConfiguration configuration, Action<dynamic> callback) : base(configuration.path, @"netLog", (result) => HandleNetLogLine(result, callback))
        {
        }

        private static Regex SystemRegex = new Regex(@"^{([0-9][0-9]:[0-9][0-9]:[0-9][0-9])} System:([0-9]+)\(([^\)]+)\).* ([A-Za-z]+)$");
        private static void HandleNetLogLine(string line, Action<dynamic> callback)
        {
            Match match = SystemRegex.Match(line);
            if (match.Success)
            {
                if (@"Training" == match.Groups[3].Value || @"Destination" == match.Groups[3].Value)
                {
                    // We ignore training missions
                    return;
                }

                if (@"ProvingGround" == match.Groups[4].Value)
                {
                    // We ignore CQC
                    return;
                }

                dynamic result = new JObject();
                result.type = "Location";
                result.starsystem = match.Groups[3].Value;
                result.environment = match.Groups[4].Value;
                callback(result);
            }
        }
    }
}
