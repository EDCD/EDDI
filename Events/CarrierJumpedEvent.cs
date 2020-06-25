using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiEvents
{
    public class CarrierJumpedEvent : Event
    {
        public const string NAME = "Carrier jumped";
        public const string DESCRIPTION = "Triggered when you are docked at a fleet carrier as it completes a jump";
        public const string SAMPLE = "{\"timestamp\":\"2020-05-17T14:07:24Z\",\"event\":\"CarrierJump\",\"Docked\":true,\"StationName\":\"G53-K3Q\",\"StationType\":\"FleetCarrier\",\"MarketID\":3700571136,\"StationFaction\":{\"Name\":\"FleetCarrier\"},\"StationGovernment\":\"$government_Carrier;\",\"StationGovernment_Localised\":\"Private Ownership \",\"StationServices\":[\"dock\",\"autodock\",\"blackmarket\",\"commodities\",\"contacts\",\"exploration\",\"crewlounge\",\"rearm\",\"refuel\",\"repair\",\"engineer\",\"flightcontroller\",\"stationoperations\",\"stationMenu\",\"carriermanagement\",\"carrierfuel\",\"voucherredemption\"],\"StationEconomy\":\"$economy_Carrier;\",\"StationEconomy_Localised\":\"Private Enterprise\",\"StationEconomies\":[{\"Name\":\"$economy_Carrier;\",\"Name_Localised\":\"Private Enterprise\",\"Proportion\":1}],\"StarSystem\":\"Aparctias\",\"SystemAddress\":358797513434,\"StarPos\":[25.1875,-56.375,22.90625],\"SystemAllegiance\":\"Independent\",\"SystemEconomy\":\"$economy_Colony;\",\"SystemEconomy_Localised\":\"Colony\",\"SystemSecondEconomy\":\"$economy_Refinery;\",\"SystemSecondEconomy_Localised\":\"Refinery\",\"SystemGovernment\":\"$government_Dictatorship;\",\"SystemGovernment_Localised\":\"Dictatorship\",\"SystemSecurity\":\"$SYSTEM_SECURITY_medium;\",\"SystemSecurity_Localised\":\"Medium Security\",\"Population\":80000,\"Body\":\"Aparctias\",\"BodyID\":0,\"BodyType\":\"Star\",\"Powers\":[\"Yuri Grom\"],\"PowerplayState\":\"Exploited\",\"Factions\":[{\"Name\":\"Union of Aparctias Future\",\"FactionState\":\"None\",\"Government\":\"Democracy\",\"Influence\":0.062,\"Allegiance\":\"Federation\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0},{\"Name\":\"Monarchy of Aparctias\",\"FactionState\":\"None\",\"Government\":\"Feudal\",\"Influence\":0.035,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0},{\"Name\":\"Aparctias Purple Council\",\"FactionState\":\"Boom\",\"Government\":\"Anarchy\",\"Influence\":0.049,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"ActiveStates\":[{\"State\":\"Boom\"}]},{\"Name\":\"Beta-3 Tucani Silver Allied Net\",\"FactionState\":\"None\",\"Government\":\"Corporate\",\"Influence\":0.096,\"Allegiance\":\"Federation\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0.32538},{\"Name\":\"Falcons' Nest\",\"FactionState\":\"None\",\"Government\":\"Confederacy\",\"Influence\":0.078,\"Allegiance\":\"Federation\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"RecoveringStates\":[{\"State\":\"NaturalDisaster\",\"Trend\":0}]},{\"Name\":\"EG Union\",\"FactionState\":\"War\",\"Government\":\"Dictatorship\",\"Influence\":0.34,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"ActiveStates\":[{\"State\":\"Boom\"},{\"State\":\"War\"}]},{\"Name\":\"Paladin Consortium\",\"FactionState\":\"War\",\"Government\":\"Democracy\",\"Influence\":0.34,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"PendingStates\":[{\"State\":\"Boom\",\"Trend\":0},{\"State\":\"CivilLiberty\",\"Trend\":0}],\"ActiveStates\":[{\"State\":\"War\"}]}],\"SystemFaction\":{\"Name\":\"EG Union\",\"FactionState\":\"War\"},\"Conflicts\":[{\"WarType\":\"war\",\"Status\":\"active\",\"Faction1\":{\"Name\":\"EG Union\",\"Stake\":\"Hancock Refinery\",\"WonDays\":1},\"Faction2\":{\"Name\":\"Paladin Consortium\",\"Stake\":\"\",\"WonDays\":0}}]}";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CarrierJumpedEvent()
        {
            // System variables
            VARIABLES.Add("systemname", "The name of the system into which the carrier has jumped");
            VARIABLES.Add("x", "The X co-ordinate of the system into which the carrier has jumped");
            VARIABLES.Add("y", "The Y co-ordinate of the system into which the carrier has jumped");
            VARIABLES.Add("z", "The Z co-ordinate of the system into which the carrier has jumped");
            VARIABLES.Add("economy", "The economy of the system into which the carrier has jumped, if any");
            VARIABLES.Add("economy2", "The secondary economy of the system into which the carrier has jumped, if any");
            VARIABLES.Add("security", "The security of the system into which the carrier has jumped, if any");
            VARIABLES.Add("population", "The population of the system into which the carrier has jumped, if any");
            VARIABLES.Add("systemfaction", "The faction controlling the system into which the carrier has jumped, if any");
            VARIABLES.Add("systemstate", "The state of the faction controlling the system into which the carrier has jumped, if any");
            VARIABLES.Add("systemgovernment", "The government of the system into which the carrier has jumped, if any");
            VARIABLES.Add("systemallegiance", "The allegiance of the system into which the carrier has jumped, if any");
            VARIABLES.Add("factions", "A list of faction objects describing the factions in the system, if any");
            VARIABLES.Add("conflicts", "A list of conflict objects describing any conflicts between factions in the system, if any");
            VARIABLES.Add("power", "(Only when pledged) The powerplay power exerting influence over the star system. If the star system is `Contested`, this will be empty");
            VARIABLES.Add("powerstate", "(Only when pledged) The state of powerplay efforts within the star system, if any");

            // Body variables
            VARIABLES.Add("bodyname", "The nearest body to the carrier, if any");
            VARIABLES.Add("bodytype", "The type of the body nearest to the carrier, if any");
            VARIABLES.Add("shortname", "The short name of the nearest body, if any");

            // Carrier variables
            VARIABLES.Add("carriername", "The name of the carrier");
        }

        // System variables

        public string systemname { get; private set; }

        public decimal x { get; private set; }

        public decimal y { get; private set; }

        public decimal z { get; private set; }

        public string economy => systemEconomy?.localizedName;

        public string economy2 => systemEconomy2?.localizedName;

        public string security => securityLevel?.localizedName;

        public long? population { get; private set; }

        public string systemfaction => controllingsystemfaction?.name;

        public string systemstate => (controllingsystemfaction?.presences.FirstOrDefault(p => p.systemName == systemname)?.FactionState ?? FactionState.None).localizedName;

        public string systemgovernment => (controllingsystemfaction?.Government ?? Government.None).localizedName;

        public string systemallegiance => (controllingsystemfaction?.Allegiance ?? Superpower.None).localizedName;

        public List<Faction> factions { get; private set; }

        public List<Conflict> conflicts { get; private set; }

        // Powerplay properties (only when pledged)

        public string power => Power?.localizedName;

        public string powerstate => powerState?.localizedName;

        // Body variables

        public string bodyname { get; private set; }

        public string bodytype => bodyType?.localizedName;

        public string shortname => (systemname == null || bodyname == systemname) ? bodyname : bodyname.Replace(systemname, "").Trim();

        // Carrier variables

        public string carriername { get; private set; }

        // These properties are not intended to be user facing
        public bool docked { get; private set; }
        public long? systemAddress { get; private set; }
        public long? carrierId { get; private set; }
        public Economy systemEconomy { get; private set; }
        public Economy systemEconomy2 { get; private set; }
        public Faction controllingsystemfaction { get; private set; }
        public SecurityLevel securityLevel { get; private set; }
        public BodyType bodyType { get; private set; }
        public long? bodyId { get; private set; }
        public Power Power { get; private set; }
        public PowerplayState powerState { get; private set; }
        public Faction carrierFaction { get; private set; }
        public StationModel carrierType { get; private set; }
        public List<StationService> carrierServices { get; private set; }
        public List<EconomyShare> carrierEconomies { get; private set; }

        public CarrierJumpedEvent(DateTime timestamp, string systemName, long systemAddress, decimal x, decimal y, decimal z,
            string bodyName, long? bodyId, BodyType bodyType, Faction systemFaction, List<Faction> factions, List<Conflict> conflicts,
            Economy systemEconomy, Economy systemEconomy2, SecurityLevel systemSecurity, long? systemPopulation, Power powerplayPower,
            PowerplayState powerplayState, bool docked, string carrierName, StationModel carrierType, long? carrierId, Faction stationFaction,
            List<StationService> stationServices, List<EconomyShare> stationEconomies) : base(timestamp, NAME)
        {
            // System
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.x = x;
            this.y = y;
            this.z = z;
            this.controllingsystemfaction = systemFaction;
            this.systemEconomy = (systemEconomy ?? Economy.None);
            this.systemEconomy2 = (systemEconomy2 ?? Economy.None);
            this.securityLevel = systemSecurity ?? SecurityLevel.None;
            this.population = systemPopulation;
            this.factions = factions ?? new List<Faction>();
            this.conflicts = conflicts ?? new List<Conflict>();
            this.Power = powerplayPower;
            this.powerState = powerplayState ?? PowerplayState.None;

            // Body
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.bodyType = bodyType ?? BodyType.None;

            // Carrier
            this.docked = docked;
            this.carrierId = carrierId;
            this.carriername = carrierName;
            this.carrierType = carrierType ?? StationModel.FromEDName("FleetCarrier");
            this.carrierFaction = stationFaction;
            this.carrierServices = stationServices ?? new List<StationService>();
            this.carrierEconomies = stationEconomies ?? new List<EconomyShare>();
        }
    }
}