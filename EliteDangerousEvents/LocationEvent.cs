using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class LocationEvent : Event
    {
        public const string NAME = "Location";
        public const string DESCRIPTION = "Triggered when the commander's location is reported";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-28T10:54:07Z\", \"event\":\"Location\", \"Docked\":true, \"StationName\":\"Jameson Memorial\", \"StationType\":\"Orbis\", \"StarSystem\":\"Shinrarta Dezhra\", \"StarPos\":[55.719,17.594,27.156], \"Allegiance\":\"Independent\", \"Economy\":\"$economy_HighTech;\", \"Economy_Localised\":\"High tech\", \"Government\":\"$government_Democracy;\", \"Government_Localised\":\"Democracy\", \"Security\":\"$SYSTEM_SECURITY_high;\", \"Security_Localised\":\"High Security\", \"Body\":\"Jameson Memorial\", \"Faction\":\"The Pilots Federation\", \"FactionState\":\"Boom\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static LocationEvent()
        {
            VARIABLES.Add("system", "The name of the system in which the commander resides");
            VARIABLES.Add("x", "The X co-ordinate of the system in which the commander resides");
            VARIABLES.Add("y", "The Y co-ordinate of the system in which the commander resides");
            VARIABLES.Add("z", "The Z co-ordinate of the system in which the commander resides");
            VARIABLES.Add("body", "The nearest body to the commander");
            VARIABLES.Add("bodytype", "The type of the nearest body to the commander");
            VARIABLES.Add("docked", "True if the commander is docked");
            VARIABLES.Add("allegiance", "The allegiance of the system in which the commander resides");
            VARIABLES.Add("faction", "The faction controlling the system in which the commander resides");
            VARIABLES.Add("factionstate", "The state of the faction controlling the system in which the commander resides");
            VARIABLES.Add("economy", "The economy of the system in which the commander resides");
            VARIABLES.Add("government", "The government of the system in which the commander resides");
            VARIABLES.Add("security", "The security of the system in which the commander resides");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("x")]
        public decimal x { get; private set; }

        [JsonProperty("y")]
        public decimal y { get; private set; }

        [JsonProperty("z")]
        public decimal z { get; private set; }

        [JsonProperty("body")]
        public string body { get; private set; }

        [JsonProperty("bodytype")]
        public string bodytype { get; private set; }

        [JsonProperty("docked")]
        public bool docked { get; private set; }

        [JsonProperty("allegiance")]
        public string allegiance { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("factionstate")]
        public string factionstate { get; private set; }

        [JsonProperty("economy")]
        public string economy { get; private set; }

        [JsonProperty("government")]
        public string government { get; private set; }

        [JsonProperty("security")]
        public string security { get; private set; }

        public LocationEvent(DateTime timestamp, string system, decimal x, decimal y, decimal z, string body, string bodytype, bool docked, Superpower allegiance, string faction, State factionstate, Economy economy, Government government, SecurityLevel security) : base(timestamp, NAME)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
            this.body = body;
            this.bodytype = bodytype;
            this.docked = docked;
            this.allegiance = (allegiance == null ? Superpower.None.name : allegiance.name);
            this.faction = faction;
            this.factionstate = (factionstate == null ? State.None.name : factionstate.name);
            this.economy = (economy == null ? Economy.None.name : economy.name);
            this.government = (government == null ? Government.None.name : government.name);
            this.security = (security == null ? SecurityLevel.Low.name : security.name);
        }
    }
}
