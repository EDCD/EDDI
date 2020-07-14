using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiEvents
{
    public abstract class Event
    {
        /// <summary> The raw event from which this event was obtained.  This is optional </summary>
        [JsonProperty("raw"), VoiceAttackIgnore]
        public string raw { get; set; }

        [JsonProperty("timestamp")]
        public DateTime timestamp { get; private set; }

        /// <summary> The EDDI event triggered by this event.</summary>
        [JsonProperty("type"), VoiceAttackIgnore]
        public string type { get; private set; }

        /// <summary> Whether this event was triggered during the initial journal load at launch or later.</summary>
        [JsonProperty("fromLoad"), VoiceAttackIgnore]
        public bool fromLoad { get; set; }

        public Event(DateTime timestamp, string type)
        {
            this.timestamp = timestamp;
            this.type = type;
        }
    }
}
