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
            VARIABLES.Add("commander", "The commander's name");
            VARIABLES.Add("ship", "The commander's ship");
            VARIABLES.Add("shipid", "The ID of the commander's ship");
            VARIABLES.Add("mode", "The game mode (Open, Group or Solo)");
            VARIABLES.Add("group", "The name of the group (only if mode == Group)");
            VARIABLES.Add("credits", "The number of credits the commander has");
            VARIABLES.Add("loan", "The current loan the commander has");
        }

        [JsonProperty("commander")]
        public string commander { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonIgnore]
        public Ship Ship { get; private set; }

        [JsonProperty("mode")]
        public string mode { get; private set; }

        [JsonProperty("group")]
        public string group { get; private set; }

        [JsonProperty("credits")]
        public decimal credits { get; private set; }

        [JsonProperty("loan")]
        public decimal loan { get; private set; }

        public CommanderContinuedEvent(DateTime timestamp, string commander, Ship ship, GameMode mode, string group, decimal credits, decimal loan) : base(timestamp, NAME)
        {
            this.commander = commander;
            this.Ship = ship;
            this.ship = (ship == null ? null : ship.model);
            this.shipid = (ship == null ? (int?)null : ship.LocalId);
            this.mode = (mode == null ? null : mode.name);
            this.group = group;
            this.credits = credits;
            this.loan = loan;
        }
    }
}
