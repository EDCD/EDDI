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
        public static JumpedEvent SAMPLE = new JumpedEvent(DateTime.Now, "LP 98-132", -26.78125M, 37.03125M, -4.59375M, Superpower.Federation, "Brotherhood of LP 98-132", State.Outbreak, Economy.Extraction, Government.Anarchy, SecurityLevel.High);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static LocationEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-07-21T13:16:49Z\",\"event\":\"FSDJump\",\"StarSystem\":\"LP 98-132\",\"StarPos\":[-26.781,37.031,-4.594],\"Economy\":\"$economy_Extraction;\",“Allegiance”:”Federation”,\"Government\":\"$government_Anarchy;\",\"Security\":”$SYSTEM_SECURITY_high_anarchy;”,\"JumpDist\":5.230,\"FuelUsed\":0.355614,\"FuelLevel\":12.079949,\"Faction\":\"Brotherhood of LP 98-132\",\"FactionState\":\"Outbreak\"}";

            VARIABLES.Add("system", "The name of the system in which the commander resides");
            VARIABLES.Add("x", "The X co-ordinate of the system in which the commander resides");
            VARIABLES.Add("y", "The Y co-ordinate of the system in which the commander resides");
            VARIABLES.Add("z", "The Z co-ordinate of the system in which the commander resides");
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

        [JsonProperty("allegiance")]
        public Superpower allegiance { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("factionstate")]
        public State factionstate { get; private set; }

        [JsonProperty("economy")]
        public Economy economy { get; private set; }

        [JsonProperty("government")]
        public Government government { get; private set; }

        [JsonProperty("security")]
        public SecurityLevel security { get; private set; }

        public LocationEvent(DateTime timestamp, string system, decimal x, decimal y, decimal z, Superpower allegiance, string faction, State factionstate, Economy economy, Government government, SecurityLevel security) : base(timestamp, NAME)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
            this.allegiance = allegiance;
            this.faction = faction;
            this.factionstate = factionstate;
            this.economy = economy;
            this.government = government;
            this.security = security;
        }
    }
}
