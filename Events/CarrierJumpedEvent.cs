using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierJumpedEvent : Event
    {
        public const string NAME = "Carrier jumped";
        public const string DESCRIPTION = "Triggered when you are docked at a fleet carrier as it completes a jump";
        public static readonly string[] SAMPLES =
        {
            "{ \"timestamp\":\"2020-05-17T14:07:24Z\",\"event\":\"CarrierJump\",\"Docked\":true,\"StationName\":\"G53-K3Q\",\"StationType\":\"FleetCarrier\",\"MarketID\":3700571136,\"StationFaction\":{\"Name\":\"FleetCarrier\"},\"StationGovernment\":\"$government_Carrier;\",\"StationGovernment_Localised\":\"Private Ownership \",\"StationServices\":[\"dock\",\"autodock\",\"blackmarket\",\"commodities\",\"contacts\",\"exploration\",\"crewlounge\",\"rearm\",\"refuel\",\"repair\",\"engineer\",\"flightcontroller\",\"stationoperations\",\"stationMenu\",\"carriermanagement\",\"carrierfuel\",\"voucherredemption\"],\"StationEconomy\":\"$economy_Carrier;\",\"StationEconomy_Localised\":\"Private Enterprise\",\"StationEconomies\":[{\"Name\":\"$economy_Carrier;\",\"Name_Localised\":\"Private Enterprise\",\"Proportion\":1}],\"StarSystem\":\"Aparctias\",\"SystemAddress\":358797513434,\"StarPos\":[25.1875,-56.375,22.90625],\"SystemAllegiance\":\"Independent\",\"SystemEconomy\":\"$economy_Colony;\",\"SystemEconomy_Localised\":\"Colony\",\"SystemSecondEconomy\":\"$economy_Refinery;\",\"SystemSecondEconomy_Localised\":\"Refinery\",\"SystemGovernment\":\"$government_Dictatorship;\",\"SystemGovernment_Localised\":\"Dictatorship\",\"SystemSecurity\":\"$SYSTEM_SECURITY_medium;\",\"SystemSecurity_Localised\":\"Medium Security\",\"Population\":80000,\"Body\":\"Aparctias\",\"BodyID\":0,\"BodyType\":\"Star\",\"Powers\":[\"Yuri Grom\"],\"PowerplayState\":\"Exploited\",\"Factions\":[{\"Name\":\"Union of Aparctias Future\",\"FactionState\":\"None\",\"Government\":\"Democracy\",\"Influence\":0.062,\"Allegiance\":\"Federation\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0},{\"Name\":\"Monarchy of Aparctias\",\"FactionState\":\"None\",\"Government\":\"Feudal\",\"Influence\":0.035,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0},{\"Name\":\"Aparctias Purple Council\",\"FactionState\":\"Boom\",\"Government\":\"Anarchy\",\"Influence\":0.049,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"ActiveStates\":[{\"State\":\"Boom\"}]},{\"Name\":\"Beta-3 Tucani Silver Allied Net\",\"FactionState\":\"None\",\"Government\":\"Corporate\",\"Influence\":0.096,\"Allegiance\":\"Federation\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0.32538},{\"Name\":\"Falcons' Nest\",\"FactionState\":\"None\",\"Government\":\"Confederacy\",\"Influence\":0.078,\"Allegiance\":\"Federation\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"RecoveringStates\":[{\"State\":\"NaturalDisaster\",\"Trend\":0}]},{\"Name\":\"EG Union\",\"FactionState\":\"War\",\"Government\":\"Dictatorship\",\"Influence\":0.34,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"ActiveStates\":[{\"State\":\"Boom\"},{\"State\":\"War\"}]},{\"Name\":\"Paladin Consortium\",\"FactionState\":\"War\",\"Government\":\"Democracy\",\"Influence\":0.34,\"Allegiance\":\"Independent\",\"Happiness\":\"$Faction_HappinessBand2;\",\"Happiness_Localised\":\"Happy\",\"MyReputation\":0,\"PendingStates\":[{\"State\":\"Boom\",\"Trend\":0},{\"State\":\"CivilLiberty\",\"Trend\":0}],\"ActiveStates\":[{\"State\":\"War\"}]}],\"SystemFaction\":{\"Name\":\"EG Union\",\"FactionState\":\"War\"},\"Conflicts\":[{\"WarType\":\"war\",\"Status\":\"active\",\"Faction1\":{\"Name\":\"EG Union\",\"Stake\":\"Hancock Refinery\",\"WonDays\":1},\"Faction2\":{\"Name\":\"Paladin Consortium\",\"Stake\":\"\",\"WonDays\":0}}]}",
            "{ \"timestamp\":\"2024-10-19T19:54:13Z\", \"event\":\"CarrierJump\", \"Docked\":true, \"StationName\":\"G53-K3Q\", \"StationType\":\"FleetCarrier\", \"MarketID\":3700571136, \"StationFaction\":{ \"Name\":\"FleetCarrier\" }, \"StationGovernment\":\"$government_Carrier;\", \"StationGovernment_Localised\":\"Private Ownership\", \"StationServices\":[ \"dock\", \"autodock\", \"commodities\", \"contacts\", \"crewlounge\", \"engineer\", \"flightcontroller\", \"stationoperations\", \"stationMenu\", \"carriermanagement\", \"carrierfuel\", \"socialspace\" ], \"StationEconomy\":\"$economy_Carrier;\", \"StationEconomy_Localised\":\"Private Enterprise\", \"StationEconomies\":[ { \"Name\":\"$economy_Carrier;\", \"Name_Localised\":\"Private Enterprise\", \"Proportion\":1.000000 } ], \"Taxi\":false, \"Multicrew\":false, \"StarSystem\":\"HR 6421\", \"SystemAddress\":27076330676, \"StarPos\":[-114.78125,80.71875,-4.87500], \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_Industrial;\", \"SystemEconomy_Localised\":\"Industrial\", \"SystemSecondEconomy\":\"$economy_Refinery;\", \"SystemSecondEconomy_Localised\":\"Refinery\", \"SystemGovernment\":\"$government_Democracy;\", \"SystemGovernment_Localised\":\"Democracy\", \"SystemSecurity\":\"$SYSTEM_SECURITY_medium;\", \"SystemSecurity_Localised\":\"Medium Security\", \"Population\":12167638, \"Body\":\"HR 6421 4 a\", \"BodyID\":18, \"BodyType\":\"Planet\", \"Factions\":[ { \"Name\":\"HR 6421 Progressive Party\", \"FactionState\":\"CivilWar\", \"Government\":\"Democracy\", \"Influence\":0.219512, \"Allegiance\":\"Federation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":94.000000, \"HomeSystem\":true, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"League of HR 6421 Dominion\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.079268, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":54.556801 }, { \"Name\":\"HR 6421 United Solutions\", \"FactionState\":\"War\", \"Government\":\"Corporate\", \"Influence\":0.173780, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":35.049999, \"ActiveStates\":[ { \"State\":\"War\" } ] }, { \"Name\":\"Traditional HR 6421 Law Party\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.108740, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":74.720001 }, { \"Name\":\"Co-operative of HIP 82896\", \"FactionState\":\"War\", \"Government\":\"Cooperative\", \"Influence\":0.173780, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":27.235001, \"ActiveStates\":[ { \"State\":\"War\" } ] }, { \"Name\":\"HR 6421 Purple Partnership\", \"FactionState\":\"None\", \"Government\":\"Anarchy\", \"Influence\":0.025407, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":7.420000 }, { \"Name\":\"Children of Raxxla\", \"FactionState\":\"CivilWar\", \"Government\":\"Anarchy\", \"Influence\":0.219512, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":100.000000, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] } ], \"SystemFaction\":{ \"Name\":\"HR 6421 Progressive Party\", \"FactionState\":\"CivilWar\" }, \"Conflicts\":[ { \"WarType\":\"civilwar\", \"Status\":\"active\", \"Faction1\":{ \"Name\":\"HR 6421 Progressive Party\", \"Stake\":\"Serebrov Terminal\", \"WonDays\":0 }, \"Faction2\":{ \"Name\":\"Children of Raxxla\", \"Stake\":\"Julian Legacy\", \"WonDays\":1 } }, { \"WarType\":\"war\", \"Status\":\"active\", \"Faction1\":{ \"Name\":\"HR 6421 United Solutions\", \"Stake\":\"Koh Cultivation Estate\", \"WonDays\":1 }, \"Faction2\":{ \"Name\":\"Co-operative of HIP 82896\", \"Stake\":\"\", \"WonDays\":1 } } ] }"
        };

        // System variables

        [PublicAPI("The name of the system into which the carrier has jumped")]
        public string systemname { get; private set; }

        [PublicAPI( "The numeric system address of the system into which the carrier has jumped" )]
        public ulong systemAddress { get; private set; }

        [PublicAPI("The X co-ordinate of the system into which the carrier has jumped")]
        public decimal x { get; private set; }

        [PublicAPI("The Y co-ordinate of the system into which the carrier has jumped")]
        public decimal y { get; private set; }

        [PublicAPI("The Z co-ordinate of the system into which the carrier has jumped")]
        public decimal z { get; private set; }

        [PublicAPI("The economy of the system into which the carrier has jumped, if any")]
        public string economy => systemEconomy?.localizedName;

        [PublicAPI("The secondary economy of the system into which the carrier has jumped, if any")]
        public string economy2 => systemEconomy2?.localizedName;

        [PublicAPI("The security of the system into which the carrier has jumped, if any")]
        public string security => securityLevel?.localizedName;

        [PublicAPI("The population of the system into which the carrier has jumped, if any")]
        public long? population { get; private set; }

        [PublicAPI("The faction controlling the system into which the carrier has jumped, if any")]
        public string systemfaction => controllingsystemfaction?.name;

        [PublicAPI("The state of the faction controlling the system into which the carrier has jumped, if any")]
        public string systemstate => (controllingsystemfaction?.presences.FirstOrDefault(p => p.systemAddress == systemAddress )?.FactionState ?? FactionState.None).localizedName;

        [PublicAPI("The government of the system into which the carrier has jumped, if any")]
        public string systemgovernment => (controllingsystemfaction?.Government ?? Government.None).localizedName;

        [PublicAPI("The allegiance of the system into which the carrier has jumped, if any")]
        public string systemallegiance => (controllingsystemfaction?.Allegiance ?? Superpower.None).localizedName;

        [PublicAPI("A list of faction objects describing the factions in the system, if any")]
        public List<Faction> factions { get; private set; }

        [PublicAPI("A list of conflict objects describing any conflicts between factions in the system, if any")]
        public List<Conflict> conflicts { get; private set; }

        // Powerplay properties

        [PublicAPI( "The localized powerplay power controlling the star system, if any.  If the star system is `Unoccupied` or `Contested` then this will be empty." )]
        public string power => ( Power ?? Power.None ).localizedName;

        [PublicAPI( "The localized names of powerplay powers having star systems with acquisition radii which overlap the star system, less the controlling power, if any" )]
        public List<string> nearbypowers => NearbyPowers?
            .Select( p => p.localizedName )
            .ToList();

        [PublicAPI( "The localized names of powerplay powers with at least 30% progress towards acquiring control of an uncontrolled star system, if any, in descending order" )]
        public List<string> contestingpowers => ContestingPowers?
            .Select( p => p.localizedName )
            .ToList();

        [PublicAPI( "The localized state of powerplay efforts within the star system" )]
        public string powerstate => ( PowerState ?? PowerplayState.Unoccupied ).localizedName;

        [PublicAPI( "The powerplay power controlling the star system, if any, as an object." )]
        public Power Power { get; private set; }

        [PublicAPI( "Powerplay powers having star systems with acquisition radii which overlap the star system, less the controlling power, if any, as objects" )]
        public List<Power> NearbyPowers { get; set; }

        [PublicAPI( "Powerplay powers with at least 30% progress towards acquiring control of an uncontrolled star system, if any, as objects, in descending order" )]
        public List<Power> ContestingPowers => powerAcquisitionProgress?
            .Where( kvp => kvp.Value >= 30 )
            .Select( kvp => kvp.Key )
            .ToList() ?? new List<Power>();

        [PublicAPI( "The state of powerplay efforts within the star system, as an object" )]
        public PowerplayState PowerState { get; private set; }

        [PublicAPI( "The progress of nearby powerplay powers towards obtaining control of the star system, as a list of objects where the key is the power (as an object) and the value is the percent progress towards achieving the control score threshold" )]
        public Dictionary<Power, decimal> powerAcquisitionProgress { get; private set; }

        [PublicAPI( "The percent progress of the controlling power, if any, in consolidating control over the star system. Values below 0% indicate a reduction in the control level at the end of the cycle while values above 100% indicate an increase in the control level at the end of the cycle (if the current control state is less than 'Stronghold')" )]
        public decimal powerControlProgress { get; set; }

        [PublicAPI( "The control points accumulated by the controlling power via powerplay reinforcement activities during the current cycle" )]
        public int powerReinforcementControlPoints { get; set; }

        [PublicAPI( "The control points lost by the controlling power via powerplay undermining activities during the current cycle" )]
        public int powerUnderminingControlPoints { get; set; }

        // Body variables

        [PublicAPI("The nearest body to the carrier, if any")]
        public string bodyname { get; private set; }

        [PublicAPI( "The numeric ID of the body nearest body to the carrier, if any" )]
        public long? bodyId { get; private set; }

        [PublicAPI("The type of the body nearest to the carrier, if any")]
        public string bodytype => bodyType?.localizedName;

        [PublicAPI("The short name of the nearest body, if any")]
        public string shortname => Body.GetShortName(bodyname, systemname);

        // Carrier variables

        [PublicAPI("The name of the carrier")]
        public string carriername { get; private set; }

        // Thargoid War
        [PublicAPI( "Thargoid war data, when applicable" )]
        public ThargoidWar ThargoidWar { get; private set; }

        // These properties are not intended to be user facing
        public bool docked { get; private set; }

        public bool onFoot { get; private set; }

        public long? carrierID { get; private set; }

        public Economy systemEconomy { get; private set; }

        public Economy systemEconomy2 { get; private set; }

        public Faction controllingsystemfaction { get; private set; }

        public SecurityLevel securityLevel { get; private set; }

        public BodyType bodyType { get; private set; }
        
        public Faction carrierFaction { get; private set; }

        public StationModel carrierType { get; private set; }

        public List<StationService> carrierServices { get; private set; }

        public List<EconomyShare> carrierEconomies { get; private set; }

        public CarrierJumpedEvent ( DateTime timestamp, string systemName, ulong systemAddress, decimal x, decimal y,
            decimal z, string bodyName, long? bodyId, BodyType bodyType, bool docked, bool onFoot, string carrierName,
            StationModel carrierType, long? carrierId, List<StationService> stationServices, Faction systemFaction,
            Faction stationFaction, List<Faction> factions, List<Conflict> conflicts,
            List<EconomyShare> stationEconomies, Economy systemEconomy, Economy systemEconomy2,
            SecurityLevel systemSecurity, long? systemPopulation, Power controllingPower, List<Power> powerplayPowers,
            PowerplayState powerplayState, Dictionary<Power, decimal> powerAcquisitionProgress,
            decimal powerplayControlProgress, int powerplayReinforcementControlPoints,
            int powerplayUnderminingControlPoints, ThargoidWar thargoidWar ) : base( timestamp, NAME )
        {
            // System
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.x = x;
            this.y = y;
            this.z = z;
            this.controllingsystemfaction = systemFaction;
            this.systemEconomy = ( systemEconomy ?? Economy.None );
            this.systemEconomy2 = ( systemEconomy2 ?? Economy.None );
            this.securityLevel = systemSecurity ?? SecurityLevel.None;
            this.population = systemPopulation;
            this.factions = factions ?? new List<Faction>();
            this.conflicts = conflicts ?? new List<Conflict>();
            this.ThargoidWar = thargoidWar;

            // Body
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.bodyType = bodyType ?? BodyType.None;

            // Carrier
            this.docked = docked;
            this.onFoot = onFoot;
            this.carrierID = carrierId;
            this.carriername = carrierName;
            this.carrierType = carrierId is null ? null : carrierType ?? StationModel.FleetCarrier;
            this.carrierFaction = stationFaction;
            this.carrierServices = stationServices ?? new List<StationService>();
            this.carrierEconomies = stationEconomies ?? new List<EconomyShare>();

            // Powerplay
            this.Power = controllingPower;
            this.NearbyPowers = powerplayPowers?
                .Where( p => p.edname != Power?.edname )
                .ToList();
            this.PowerState = powerplayState ?? PowerplayState.Unoccupied;
            this.powerAcquisitionProgress = powerAcquisitionProgress;
            this.powerControlProgress = powerplayControlProgress;
            this.powerReinforcementControlPoints = powerplayReinforcementControlPoints;
            this.powerUnderminingControlPoints = powerplayUnderminingControlPoints;
        }
    }
}