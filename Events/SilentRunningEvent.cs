using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SilentRunningEvent : Event
    {
        public const string NAME = "Silent running";
        public const string DESCRIPTION = "Triggered when you activate or deactivate silent running";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SilentRunningEvent()
        {
            VARIABLES.Add("silentrunning", "A boolean value. True if silent running is active.");
        }

        [JsonProperty("silentrunning")]
        public bool silentrunning { get; private set; }

        public SilentRunningEvent(DateTime timestamp, bool silentRunning) : base(timestamp, NAME)
        {
            this.silentrunning = silentRunning;
        }
    }
}
