using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class CommanderContinuedEvent : Event
    {
        public const string NAME = "Commander continued";
        public const string DESCRIPTION = "Triggered when you continue an existing game";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LoadGame\",\"Commander\":\"HRC1\",\"Ship\":\"CobraMkIII\",\"ShipID\":1,\"GameMode\":\"Group\",\"Group\":\"Mobius\",\"Credits\":600120,\"Loan\":0}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommanderContinuedEvent()
        {
            VARIABLES.Add("commander", "The name of the commander");
            VARIABLES.Add("ship", "The ship of the commander");
            VARIABLES.Add("mode", "The game mode");
            VARIABLES.Add("group", "The name of the group (only if mode == Group)");
            VARIABLES.Add("credits", "the number of credits the commander has");
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

        public CommanderContinuedEvent(DateTime timestamp, string commander, Ship ship, GameMode mode, string group, decimal credits) : base(timestamp, NAME)
        {
            this.commander = commander;
            this.ship = ship;
            this.mode = mode;
            this.group = group;
            this.credits = credits;
        }
    }
}
