using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EnteredSupercruiseEvent : Event
    {
        public const string NAME = "Entered supercruise";
        public const string DESCRIPTION = "Triggered when your ship enters supercruise";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"SupercruiseEntry\",\"StarSystem\":\"Yuetu\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EnteredSupercruiseEvent()
        {
            VARIABLES.Add("system", "The system at which the commander has entered supercruise");
            VARIABLES.Add("taxi", "True if the ship is an Apex taxi");
            VARIABLES.Add("multicrew", "True if the ship is belongs to another player");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("taxi")]
        public bool? taxi { get; private set; }

        [JsonProperty("multicrew")]
        public bool? multicrew { get; private set; }

        // Not intended to be user facing

        public long? systemAddress { get; private set; }

        public EnteredSupercruiseEvent(DateTime timestamp, string system, long? systemAddress, bool? taxi, bool? multicrew) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.taxi = taxi;
            this.multicrew = multicrew;
        }
    }
}
