using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ClearedSaveEvent : Event
    {
        public const string NAME = "Cleared save";
        public const string DESCRIPTION = "Triggered when you clear your save";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ClearSaveGame\",\"Name\":\"HRC1\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ClearedSaveEvent()
        {
            VARIABLES.Add("name", "The name of the player whose save has been cleared");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        public ClearedSaveEvent(DateTime timestamp, string name) : base(timestamp, NAME)
        {
            this.name = name;
        }
    }
}
