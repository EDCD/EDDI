using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class JumpedEvent : Event
    {
        public const string NAME = "Jumped";
        public const string DESCRIPTION = "Triggered when you complete a jump to another system";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-21T13:16:49Z\",\"event\":\"FSDJump\",\"StarSystem\":\"LP 98-132\",\"StarPos\":[-26.781,37.031,-4.594],\"Economy\":\"$economy_Extraction;\",\"Allegiance\":\"Federation\",\"Government\":\"$government_Anarchy;\",\"Security\":\"$SYSTEM_SECURITY_high_anarchy;\",\"JumpDist\":5.230,\"FuelUsed\":0.355614,\"FuelLevel\":12.079949,\"Faction\":\"Brotherhood of LP 98-132\",\"FactionState\":\"Outbreak\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static JumpedEvent()
        {
            VARIABLES.Add("system", "The name of the system to which the commander has jumped");
            VARIABLES.Add("x", "The X co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("y", "The Y co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("z", "The Z co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("allegiance", "The allegiance of the system to which the commander has jumped");
            VARIABLES.Add("faction", "The faction controlling the system to which the commander has jumped");
            VARIABLES.Add("factionstate", "The state of the faction controlling the system to which the commander has jumped");
            VARIABLES.Add("economy", "The economy of the system to which the commander has jumped");
            VARIABLES.Add("government", "The government of the system to which the commander has jumped");
            VARIABLES.Add("security", "The security of the system to which the commander has jumped");
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

        public JumpedEvent(DateTime timestamp, string system, decimal x, decimal y, decimal z, Superpower allegiance, string faction, State factionstate, Economy economy, Government government, SecurityLevel security) : base(timestamp, NAME)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
            this.allegiance = (allegiance == null ? Superpower.None.name : allegiance.name) ;
            this.faction = faction;
            this.factionstate = (factionstate == null ? State.None.name : factionstate.name);
            this.economy = (economy == null ? Economy.None.name : economy.name);
            this.government = (government == null ? Government.None.name : government.name);
            this.security = (security == null ? SecurityLevel.Low.name : security.name);
        }
    }
}
