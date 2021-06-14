using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class JumpedEvent : Event
    {
        public const string NAME = "Jumped";
        public const string DESCRIPTION = "Triggered when you complete a jump to another system";
        public const string SAMPLE = "{ \"timestamp\":\"2018-10-29T10:05:21Z\", \"event\":\"FSDJump\", \"StarSystem\":\"Eranin\", \"SystemAddress\":2832631632594, \"StarPos\":[-22.84375,36.53125,-1.18750], \"SystemAllegiance\":\"Independent\", \"SystemEconomy\":\"$economy_Agri;\", \"SystemEconomy_Localised\":\"Agriculture\", \"SystemSecondEconomy\":\"$economy_Refinery;\", \"SystemSecondEconomy_Localised\":\"Refinery\", \"SystemGovernment\":\"$government_Anarchy;\", \"SystemGovernment_Localised\":\"Anarchy\", \"SystemSecurity\":\"$GAlAXY_MAP_INFO_state_anarchy;\", \"SystemSecurity_Localised\":\"Anarchy\", \"Population\":450000, \"JumpDist\":13.334, \"FuelUsed\":0.000000, \"FuelLevel\":25.630281, \"Factions\":[ { \"Name\":\"Eranin Expeditionary Institute\", \"FactionState\":\"None\", \"Government\":\"Cooperative\", \"Influence\":0.170000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000 }, { \"Name\":\"Eranin Peoples Party\", \"FactionState\":\"CivilWar\", \"Government\":\"Communism\", \"Influence\":0.226000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":29.974300, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":82.918297 }, { \"Name\":\"Eranin Industry\", \"FactionState\":\"Outbreak\", \"Government\":\"Corporate\", \"Influence\":0.209000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand3;\", \"Happiness_Localised\":\"Discontented\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Famine\" }, { \"State\":\"Lockdown\" }, { \"State\":\"Outbreak\" } ] }, { \"Name\":\"Eranin Federal Bridge\", \"FactionState\":\"CivilWar\", \"Government\":\"Dictatorship\", \"Influence\":0.226000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Mob of Eranin\", \"FactionState\":\"CivilLiberty\", \"Government\":\"Anarchy\", \"Influence\":0.134000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand1;\", \"Happiness_Localised\":\"Elated\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Boom\" }, { \"State\":\"CivilLiberty\" } ] }, { \"Name\":\"Terran Colonial Forces\", \"FactionState\":\"CivilUnrest\", \"Government\":\"Confederacy\", \"Influence\":0.035000, \"Allegiance\":\"Alliance\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Boom\" }, { \"State\":\"CivilUnrest\" } ] } ], \"SystemFaction\":\"Mob of Eranin\", \"FactionState\":\"CivilLiberty\", \"Powers\":[ \"Edmund Mahon\" ], \"PowerplayState\":\"Exploited\" }";

        [PublicAPI("The name of the system to which the commander has jumped")]
        public string system { get; private set; }

        [PublicAPI("The X co-ordinate of the system to which the commander has jumped")]
        public decimal x { get; private set; }

        [PublicAPI("The Y co-ordinate of the system to which the commander has jumped")]
        public decimal y { get; private set; }

        [PublicAPI("The Z co-ordinate of the system to which the commander has jumped")]
        public decimal z { get; private set; }

        [PublicAPI("The name of the star at which you've arrived")]
        public string star { get; private set; }

        [PublicAPI("The distance the commander has jumped, in light years")]
        public decimal distance { get; private set; }

        [PublicAPI("The amount of fuel used in this jump")]
        public decimal fuelused { get; private set; }

        [PublicAPI("The amount of fuel remaining after this jump")]
        public decimal fuelremaining { get; private set; }

        [PublicAPI("The economy of the system to which the commander has jumped")]
        public string economy => (Economy ?? Economy.None).localizedName;

        [PublicAPI("The secondary economy of the system to which the commander has jumped, if any")]
        public string economy2 => (Economy2 ?? Economy.None).localizedName;

        [PublicAPI("The security of the system to which the commander has jumped")]
        public string security => (securityLevel ?? SecurityLevel.None).localizedName;

        [PublicAPI("The population of the system to which the commander has jumped")]
        public long? population { get; private set; }

        [PublicAPI("A list of faction objects describing the factions in the star system")]
        public List<Faction> factions { get; private set; }

        [PublicAPI("A list of conflict objects describing any conflicts between factions in the star system")]
        public List<Conflict> conflicts { get; private set; }

        [PublicAPI("True if the ship is a transport (e.g. taxi or dropship)")]
        public bool? taxi { get; private set; }

        [PublicAPI("True if the ship is belongs to another player")]
        public bool? multicrew { get; private set; }

        // Controlling faction properties

        [PublicAPI("The faction controlling the system to which the commander has jumped")]
        public string faction => controllingfaction?.name;

        [PublicAPI("The state of the faction controlling the system to which the commander has jumped")]
        public string factionstate => (controllingfaction?.presences.FirstOrDefault(p => p.systemName == system)?.FactionState ?? FactionState.None).localizedName;
        
        [PublicAPI("The allegiance of the system to which the commander has jumped")]
        public string allegiance => (controllingfaction?.Allegiance ?? Superpower.None).localizedName;

        [PublicAPI("The government of the system to which the commander has jumped")]
        public string government => (controllingfaction?.Government ?? Government.None).localizedName;

        // Powerplay properties (only when pledged)

        [PublicAPI("(Only when pledged) The powerplay power exerting influence over the star system. If the star system is `Contested`, this will be empty")]
        public string power => (Power ?? Power.None).localizedName;

        [PublicAPI("(Only when pledged) The state of powerplay efforts within the star system")]
        public string powerstate => (powerState ?? PowerplayState.None).localizedName;

        // These properties are not intended to be user facing

        public int? boostused { get; private set; }

        public long systemAddress { get; private set; }

        public Economy Economy { get; private set; } = Economy.None;

        public Economy Economy2 { get; private set; } = Economy.None;

        public Faction controllingfaction { get; private set; }

        public SecurityLevel securityLevel { get; private set; } = SecurityLevel.None;

        public FactionState factionState { get; private set; } = FactionState.None;

        public Power Power { get; private set; }

        public PowerplayState powerState { get; private set; }

        public JumpedEvent(DateTime timestamp, string system, long systemAddress, decimal x, decimal y, decimal z, string star, decimal distance, decimal fuelused, decimal fuelremaining, int? boostUsed, Faction controllingfaction, List<Faction> factions, List<Conflict> conflicts, Economy economy, Economy economy2, SecurityLevel security, long? population, Power powerplayPower, PowerplayState powerplayState, bool? taxi, bool? multicrew) : base(timestamp, NAME)
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
            this.conflicts = conflicts;
            this.Economy = (economy ?? Economy.None);
            this.Economy2 = (economy2 ?? Economy.None);
            this.securityLevel = (security ?? SecurityLevel.None);
            this.population = population;
            this.Power = powerplayPower;
            this.powerState = powerplayState;
            this.taxi = taxi;
            this.multicrew = multicrew;
        }
    }
}