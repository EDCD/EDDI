using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FSDTargetEvent : Event
    {
        public const string NAME = "Next jump";
        public const string DESCRIPTION = "Triggered when selecting a star system to jump to";
        public const string SAMPLE = @"{""timestamp"":""2019-01-29T07:13:08Z"",""event"":""FSDTarget"",""Name"":""Kuma"",""SystemAddress"":1247411177835,""RemainingJumpsInRoute"":1}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FSDTargetEvent()
        {
            VARIABLES.Add("system", "The name of the destination system");
            VARIABLES.Add("remainingjumpsinroute", "The remaining number of jumps in the current route");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("systemaddress")]
        public long systemAddress { get; private set; }

        [JsonProperty("remainingjumpsinroute")]
        public int remainingjumpsinroute { get; private set; }

        public FSDTargetEvent(DateTime timestamp, string system, long systemAddress, int remainingjumpsinroute) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.remainingjumpsinroute = remainingjumpsinroute;
        }
    }
}
