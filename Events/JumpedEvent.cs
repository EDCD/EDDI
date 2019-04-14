using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiEvents
{
    public class JumpedEvent : Event
    {
        public const string NAME = "Jumped";
        public const string DESCRIPTION = "Triggered when you complete a jump to another system";
        public const string SAMPLE = "{ \"timestamp\":\"2018-10-29T10:05:21Z\", \"event\":\"FSDJump\", \"StarSystem\":\"Eranin\", \"SystemAddress\":2832631632594, \"StarPos\":[-22.84375,36.53125,-1.18750], \"SystemAllegiance\":\"Independent\", \"SystemEconomy\":\"$economy_Agri;\", \"SystemEconomy_Localised\":\"Agriculture\", \"SystemSecondEconomy\":\"$economy_Refinery;\", \"SystemSecondEconomy_Localised\":\"Refinery\", \"SystemGovernment\":\"$government_Anarchy;\", \"SystemGovernment_Localised\":\"Anarchy\", \"SystemSecurity\":\"$GAlAXY_MAP_INFO_state_anarchy;\", \"SystemSecurity_Localised\":\"Anarchy\", \"Population\":450000, \"JumpDist\":13.334, \"FuelUsed\":0.000000, \"FuelLevel\":25.630281, \"Factions\":[ { \"Name\":\"Eranin Expeditionary Institute\", \"FactionState\":\"None\", \"Government\":\"Cooperative\", \"Influence\":0.170000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000 }, { \"Name\":\"Eranin Peoples Party\", \"FactionState\":\"CivilWar\", \"Government\":\"Communism\", \"Influence\":0.226000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":29.974300, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":82.918297 }, { \"Name\":\"Eranin Industry\", \"FactionState\":\"Outbreak\", \"Government\":\"Corporate\", \"Influence\":0.209000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand3;\", \"Happiness_Localised\":\"Discontented\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Famine\" }, { \"State\":\"Lockdown\" }, { \"State\":\"Outbreak\" } ] }, { \"Name\":\"Eranin Federal Bridge\", \"FactionState\":\"CivilWar\", \"Government\":\"Dictatorship\", \"Influence\":0.226000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Mob of Eranin\", \"FactionState\":\"CivilLiberty\", \"Government\":\"Anarchy\", \"Influence\":0.134000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand1;\", \"Happiness_Localised\":\"Elated\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Boom\" }, { \"State\":\"CivilLiberty\" } ] }, { \"Name\":\"Terran Colonial Forces\", \"FactionState\":\"CivilUnrest\", \"Government\":\"Confederacy\", \"Influence\":0.035000, \"Allegiance\":\"Alliance\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Boom\" }, { \"State\":\"CivilUnrest\" } ] } ], \"SystemFaction\":\"Mob of Eranin\", \"FactionState\":\"CivilLiberty\" }";
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
            VARIABLES.Add("destination", "The route destination system, if any");
            VARIABLES.Add("destdistance", "The distance to the destination system)");
        }

        public string system { get; private set; }

        public decimal x { get; private set; }

        public decimal y { get; private set; }

        public decimal z { get; private set; }

        /// <summary> Documented by the journal, but apparently never written. We can't rely on this being set. </summary>
        public string star { get; private set; }

        public decimal distance { get; private set; }

        public decimal fuelused { get; private set; }

        public decimal fuelremaining { get; private set; }

        public int? boostused { get; private set; }

        public string economy => (Economy ?? Economy.None).localizedName;

        public string economy2 => (Economy2 ?? Economy.None).localizedName;

        public string security => (securityLevel ?? SecurityLevel.None).localizedName;

        public long? population { get; private set; }

        public string destination { get; private set; }

        public decimal destdistance { get; private set; }

        public List<Faction> factions { get; private set; }

        // Faction properties
        public string faction => controllingfaction?.name;
        public string factionstate => (controllingfaction?.presences.FirstOrDefault(p => p.systemName == system)?.FactionState ?? FactionState.None).localizedName;
        public string allegiance => (controllingfaction?.Allegiance ?? Superpower.None).localizedName;
        public string government => (controllingfaction?.Government ?? Government.None).localizedName;

        // These properties are not intended to be user facing
        public long systemAddress { get; private set; }
        public Economy Economy { get; private set; } = Economy.None;
        public Economy Economy2 { get; private set; } = Economy.None;
        public Faction controllingfaction { get; private set; }
        public SecurityLevel securityLevel { get; private set; } = SecurityLevel.None;
        public FactionState factionState { get; private set; } = FactionState.None;

        public JumpedEvent(DateTime timestamp, string system, long systemAddress, decimal x, decimal y, decimal z, string star, decimal distance, decimal fuelused, decimal fuelremaining, int? boostUsed, Faction controllingfaction, List<Faction> factions, Economy economy, Economy economy2, SecurityLevel security, long? population, string destination, decimal destdistance) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.x = x;
            this.y = y;
            this.z = z;
            this.star = star;
            this.distance = distance;
            this.fuelused = fuelused;
            this.fuelremaining = fuelremaining;
            this.boostused = boostUsed;
            this.controllingfaction = controllingfaction;
            this.factions = factions;
            this.Economy = (economy ?? Economy.None);
            this.Economy2 = (economy2 ?? Economy.None);
            this.securityLevel = (security ?? SecurityLevel.None);
            this.population = population;
            this.destination = destination;
            this.destdistance = destdistance;
        }
    }
}