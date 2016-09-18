using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class StartedEvent : Event
    {
        public const string NAME = "Started";
        public const string DESCRIPTION = "Triggered when Elite: Dangerous starts";
        public static StartedEvent SAMPLE = new StartedEvent(DateTime.Now, "HRC1", ShipDefinitions.ShipFromEDModel("CobraMkIII"), GameMode.Group, "Mobius", 600120M);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StartedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LoadGame\",\"Commander\":\"HRC1\",\"Ship\":\"CobraMkIII\",\"ShipID\":1,\"GameMode\":\"Group\",\"Group\":\"Mobius\",\"Credits\":600120,\"Loan\":0}";

            VARIABLES.Add("commander", "The name of the commander");
            VARIABLES.Add("ship", "The ship of the commander");
            VARIABLES.Add("mode", "The game mode");
            VARIABLES.Add("group", "The name of the group (only if mode == Group)");
            VARIABLES.Add("credits", "The game mode");
        }

        [JsonProperty("commander")]
        public string commander { get; private set; }

        [JsonProperty("ship")]
        public Ship ship { get; private set; }

        [JsonProperty("mode")]
        public GameMode mode { get; private set; }

        [JsonProperty("group")]
        public string group { get; private set; }

        [JsonProperty("credits")]
        public decimal credits { get; private set; }

        public StartedEvent(DateTime timestamp, string commander, Ship ship, GameMode mode, string group, decimal credits) : base(timestamp, NAME)
        {
            this.commander = commander;
            this.ship = ship;
            this.mode = mode;
            this.group = group;
            this.credits = credits;
        }
    }
}
