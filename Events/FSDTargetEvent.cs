using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FSDTargetEvent : Event
    {
        public const string NAME = "Next jump";
        public const string DESCRIPTION = "Triggered when selecting a star system to jump to";
        public const string SAMPLE = @"{ ""timestamp"":""2020-11-14T09:19:25Z"", ""event"":""FSDTarget"", ""Name"":""Musca Dark Region CQ-Y d31"", ""SystemAddress"":1075729926531, ""StarClass"":""F"", ""RemainingJumpsInRoute"":1 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FSDTargetEvent()
        {
            VARIABLES.Add("system", "The name of the destination system");
            VARIABLES.Add("remainingjumpsinroute", "The remaining number of jumps in the current route");
            VARIABLES.Add("starclass", "The primary star's class");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("systemaddress")]
        public long systemAddress { get; private set; }

        [JsonProperty("remainingjumpsinroute")]
        public int remainingjumpsinroute { get; private set; }

        [JsonProperty("starclass")]
        public string starclass { get; private set; }

        public FSDTargetEvent(DateTime timestamp, string system, long systemAddress, int remainingjumpsinroute, string starclass) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.remainingjumpsinroute = remainingjumpsinroute;
            this.starclass = starclass;
        }
    }
}
