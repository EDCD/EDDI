using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class JumpedEvent : Event
    {
        public const string NAME = "Jumped";
        public const string DESCRIPTION = "Triggered when you complete a jump to another system";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-21T13:16:49Z\",\"event\":\"FSDJump\",\"StarSystem\":\"LP 98-132\",\"StarPos\":[-26.781,37.031,-4.594],\"SystemAddress\": 5068464268713,\"Economy\":\"$economy_Extraction;\",\"Allegiance\":\"Federation\",\"Government\":\"$government_Anarchy;\",\"Security\":\"$SYSTEM_SECURITY_high_anarchy;\", \"Population\":14087951,\"JumpDist\":5.230,\"FuelUsed\":0.355614,\"FuelLevel\":12.079949,\"Faction\":\"Brotherhood of LP 98-132\",\"FactionState\":\"Outbreak\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static JumpedEvent()
        {
            VARIABLES.Add("system", "The name of the system to which the commander has jumped");
            VARIABLES.Add("x", "The X co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("y", "The Y co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("z", "The Z co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("distance", "The distance the commander has jumped, in light years");
            VARIABLES.Add("fuelused", "The amount of fuel used in this jump");
            VARIABLES.Add("fuelremaining", "The amount of fuel remaining after this jump");
            VARIABLES.Add("allegiance", "The allegiance of the system to which the commander has jumped");
            VARIABLES.Add("faction", "The faction controlling the system to which the commander has jumped");
            VARIABLES.Add("factionstate", "The state of the faction controlling the system to which the commander has jumped");
            VARIABLES.Add("economy", "The economy of the system to which the commander has jumped");
            VARIABLES.Add("economy2", "The secondary economy of the system to which the commander has jumped, if any");
            VARIABLES.Add("government", "The government of the system to which the commander has jumped");
            VARIABLES.Add("security", "The security of the system to which the commander has jumped");
            VARIABLES.Add("population", "The population of the system to which the commander has jumped");
        }

        public string system { get; private set; }

        public decimal x { get; private set; }

        public decimal y { get; private set; }

        public decimal z { get; private set; }

        public decimal distance { get; private set; }

        public decimal fuelused { get; private set; }

        public decimal fuelremaining { get; private set; }

        public string allegiance => (Allegiance ?? Superpower.None).localizedName;

        public string faction { get; private set; }

        public string factionstate => (factionState ?? FactionState.None).localizedName;

        public string economy => (Economy ?? Economy.None).localizedName;

        public string economy2 => (Economy2 ?? Economy.None).localizedName;

        public string government => (Government ?? Government.None).localizedName;

        public string security => (securityLevel ?? SecurityLevel.None).localizedName;

        public long? population { get; private set; }

        // These properties are not intended to be user facing
        public long systemAddress { get; private set; }
        public Economy Economy { get; private set; } = Economy.None;
        public Economy Economy2 { get; private set; } = Economy.None;
        public Superpower Allegiance { get; private set; } = Superpower.None;
        public Government Government { get; private set; } = Government.None;
        public SecurityLevel securityLevel { get; private set; } = SecurityLevel.None;
        public FactionState factionState { get; private set; } = FactionState.None;

        public JumpedEvent(DateTime timestamp, string system, long systemAddress, decimal x, decimal y, decimal z, decimal distance, decimal fuelused, decimal fuelremaining, Superpower allegiance, string faction, FactionState factionstate, Economy economy, Economy economy2, Government government, SecurityLevel security, long? population) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.x = x;
            this.y = y;
            this.z = z;
            this.distance = distance;
            this.fuelused = fuelused;
            this.fuelremaining = fuelremaining;
            this.Allegiance = (allegiance ?? Superpower.None);
            this.faction = faction;
            this.factionState = (factionstate ?? FactionState.None);
            this.Economy = (economy ?? Economy.None);
            this.Economy2 = (economy2 ?? Economy.None);
            this.Government = (government ?? Government.None);
            this.securityLevel = (security ?? SecurityLevel.None);
            this.population = population;
        }
    }
}
