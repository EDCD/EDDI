using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EliteDangerousNetLogMonitor
{
    /// <summary>A log monitor for the Elite: Dangerous netlog</summary>
    public class NetLogMonitor : LogMonitor
    {
        public NetLogMonitor(string directory, Action<dynamic> callback) : base(directory, @"netLog", (result) => HandleNetLogLine(result, callback))
        {
        }

        private static Regex SystemRegex = new Regex(@"^{[0-9][0-9]:[0-9][0-9]:[0-9][0-9]} System:([0-9]+)\(([^\)]+)\).* ([A-Za-z]+)$");
        private static void HandleNetLogLine(string line, Action<dynamic> callback)
        {
            Match match = SystemRegex.Match(line);
            if (match.Success)
            {
                dynamic result = new JObject();
                result.type = "Location";
                result.starsystem = match.Groups[2].Value;
                result.environment = match.Groups[3].Value;
                callback(result);
            }
        }
    }

}
