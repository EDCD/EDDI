using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FSDEngagedEvent : Event
    {
        public const string NAME = "FSD engaged";
        public const string DESCRIPTION = "Triggered when your FSD has engaged";
        public const string SAMPLE = @"{""timestamp"":""2016-08-09T08:46:29Z"",""event"":""StartJump"",""JumpType"":""Hyperspace"",""StarClass"":""L"",""StarSystem"":""LFT 926""}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FSDEngagedEvent()
        {
            VARIABLES.Add("target", "The target frame (Supercruise/Hyperspace)");
            VARIABLES.Add("system", "The destination system (only if type is Hyperspace)");
            VARIABLES.Add("stellarclass", "The class of the destination primary star (only if type is Hyperspace)");
        }

        [JsonProperty("target")]
        public string target { get; private set; }

        [JsonProperty("stellarclass")]
        public string stellarclass { get; private set; }

        [JsonProperty("system")]
        public string system { get; private set; }

        public FSDEngagedEvent(DateTime timestamp, string jumptype, string system, string stellarclass) : base(timestamp, NAME)
        {
            this.target = jumptype;
            this.system = system;
            this.stellarclass = stellarclass;
        }
    }
}
