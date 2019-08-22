using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FSDTargetEvent : Event
    {
        public const string NAME = "Next jump";
        public const string DESCRIPTION = "Triggered when selecting a star system to jump to";
        public const string SAMPLE = @"{""timestamp"":""2019-01-29T07:13:08Z"",""event"":""FSDTarget"",""Name"":""Kuma"",""SystemAddress"":1247411177835}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FSDTargetEvent()
        {
            VARIABLES.Add("system", "The name of the destination system");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("systemaddress")]
        public long systemAddress { get; private set; }

        public FSDTargetEvent(DateTime timestamp, string system, long systemAddress) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
        }
    }
}
