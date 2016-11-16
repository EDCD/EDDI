using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class DockedEvent : Event
    {
        public const string NAME = "Docked";
        public const string DESCRIPTION = "Triggered when your ship docks at a station or outpost";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-16T09:28:19Z"", ""event"":""Docked"", ""StationName"":""Jameson Memorial"", ""StationType"":""Orbis"", ""StarSystem"":""Shinrarta Dezhra"", ""StationFaction"":""The Pilots Federation"", ""FactionState"":""Retreat"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationEconomy"":""$economy_HighTech;"", ""StationEconomy_Localised"":""High tech"" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockedEvent()
        {
            VARIABLES.Add("station", "The station at which the commander has docked");
            VARIABLES.Add("system", "The system at which the commander has docked");
            VARIABLES.Add("model", "The model of the station at which the commander has docked (Orbis, Coriolis, etc)");
            VARIABLES.Add("faction", "The faction controlling the station at which the commander has docked");
            VARIABLES.Add("factionstate", "The state of the faction controlling the station at which the commander has docked");
            VARIABLES.Add("economy", "The economy of the station at which the commander has docked");
            VARIABLES.Add("government", "The government of the station at which the commander has docked");
            VARIABLES.Add("security", "The security of the station at which the commander has docked");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("station")]
        public string station { get; private set; }

        [JsonProperty("model")]
        public string model { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("factionstate")]
        public string factionstate { get; private set; }

        [JsonProperty("economy")]
        public string economy { get; private set; }

        [JsonProperty("government")]
        public string government { get; private set; }

        public DockedEvent(DateTime timestamp, string system, string station, string model, string faction, State factionstate, Economy economy, Government government) : base(timestamp, NAME)
        {
            this.system = system;
            this.station = station;
            this.model = model;
            this.faction = faction;
            this.factionstate = (factionstate == null ? State.None.name : factionstate.name);
            this.economy = (economy == null ? Economy.None.name : economy.name);
            this.government = (government == null ? Government.None.name : government.name);
        }
    }
}
