using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class LocationEvent : Event
    {
        public const string NAME = "Location";
        public const string DESCRIPTION = "Triggered when the commander's location is reported, usually when they reload their game.";
        public const string SAMPLE = "{ \"timestamp\":\"2017-10-01T22:03:04Z\", \"event\":\"Location\", \"Docked\":true, \"StationName\":\"Heyerdahl Hub\", \"StationType\":\"Orbis\", \"StarSystem\":\"Alpha Caeli\", \"StarPos\":[45.219,-43.344,-20.125], \"SystemAddress\": 2106438175083, \"DistFromStarLS\": 1441, \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_Agri;\", \"SystemEconomy_Localised\":\"Agriculture\", \"SystemGovernment\":\"$government_Corporate;\", \"SystemGovernment_Localised\":\"Corporate\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":16796538233, \"Body\":\"Heyerdahl Hub\", \"BodyType\":\"Station\", \"MarketID\": 3223697152, \"Factions\":[ { \"Name\":\"Labour of Alpha Caeli\", \"FactionState\":\"Famine\", \"Government\":\"Democracy\", \"Influence\":0.056000, \"Allegiance\":\"Independent\", \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":0 } ] }, { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\" }, { \"Name\":\"Dangarla Creative Corp.\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.081000, \"Allegiance\":\"Federation\", \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":1 }, { \"State\":\"CivilUnrest\", \"Trend\":1 } ] }, { \"Name\":\"Alpha Caeli Gold Organisation\", \"FactionState\":\"Boom\", \"Government\":\"Corporate\", \"Influence\":0.252000, \"Allegiance\":\"Federation\" }, { \"Name\":\"Alpha Caeli Freedom Party\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.056000, \"Allegiance\":\"Independent\" }, { \"Name\":\"Blue Natural Holdings\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.438000, \"Allegiance\":\"Federation\", \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":1 } ] }, { \"Name\":\"Alpha Caeli Hand Gang\", \"FactionState\":\"Boom\", \"Government\":\"Anarchy\", \"Influence\":0.046000, \"Allegiance\":\"Independent\" }, { \"Name\":\"Aurora\", \"FactionState\":\"Boom\", \"Government\":\"Corporate\", \"Influence\":0.071000, \"Allegiance\":\"Independent\" } ], \"SystemFaction\":\"Blue Natural Holdings\", \"Powers\":[ \"Edmund Mahon\", \"Zachary Hudson\" ], \"PowerplayState\":\"Contested\" }";

        [PublicAPI("The name of the system in which the commander is located")]
        public string systemname { get; private set; }

        [PublicAPI( "The numeric system address of the star system in which the commander is located" )]
        public ulong systemAddress { get; private set; }

        [PublicAPI( "Metadata encoded into the unique 64 bit ID for the star system." )]
        public StarSystemId64 id64 => new( systemAddress );

        [PublicAPI("The X co-ordinate of the system in which the commander is located")]
        public decimal x { get; private set; }

        [PublicAPI("The Y co-ordinate of the system in which the commander is located")]
        public decimal y { get; private set; }

        [PublicAPI("The Z co-ordinate of the system in which the commander is located")]
        public decimal z { get; private set; }

        [PublicAPI("The distance of the nearest body (when close) from the main star")]
        public decimal? distancefromstar { get; private set; }

        [PublicAPI("The nearest body to the commander")]
        public string bodyname { get; private set; }

        [PublicAPI( "The numeric ID of the body nearest to the commander (if any)" )]
        public int? bodyId { get; private set; }

        [PublicAPI("The type of the nearest body to the commander")]
        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        [PublicAPI("True if the commander is docked")]
        public bool docked { get; private set; }

        [PublicAPI("The name of the station at which the commander is docked")]
        public string station { get; private set; }

        [PublicAPI("The market ID of the station at which the commander is docked")]
        public long? marketId { get; private set; }

        [PublicAPI("The type of the station at which the commander is docked")]
        public string stationtype => (stationModel ?? StationModel.None).localizedName;

        [PublicAPI("The economy of the system in which the commander resides")]
        public string economy => (Economy ?? Economy.None).localizedName;

        [PublicAPI("The secondary economy of the system in which the commander resides, if any")]
        public string economy2 => (Economy2 ?? Economy.None).localizedName;

        [PublicAPI("The security of the system in which the commander resides")]
        public string security => (securityLevel ?? SecurityLevel.None).localizedName;

        [PublicAPI("The population of the system to which the commander has jumped")]
        public long? population { get; private set; }

        [PublicAPI("The longitude of the commander (if on the ground)")]
        public decimal? longitude { get; private set; }

        [PublicAPI("The latitude of the commander (if on the ground)")]
        public decimal? latitude { get; private set; }

        // Post-3.3.03 faction properties

        [PublicAPI("The faction controlling the system in which the commander is located")]
        public string systemfaction => controllingsystemfaction?.name;

        [PublicAPI("The state of the faction controlling the system in which the commander is located")]
        public string systemstate => (controllingsystemfaction?.presences.FirstOrDefault(p => p.systemAddress == systemAddress )?.FactionState ?? FactionState.None).localizedName;

        [PublicAPI("The government of the system in which the commander is located")]
        public string systemgovernment => (controllingsystemfaction?.Government ?? Government.None).localizedName;

        [PublicAPI("The super power allegiance of the star system in which the commander is located")]
        public string systemallegiance => (controllingsystemfaction?.Allegiance ?? Superpower.None).localizedName;

        [PublicAPI("The faction controlling the station, if the commander is docked")]
        public string stationfaction => controllingstationfaction?.name;

        [PublicAPI("The state of the faction controlling the station, if the commander is docked")]
        public string stationstate => (controllingstationfaction?.presences.FirstOrDefault(p => p.systemAddress == systemAddress )?.FactionState ?? FactionState.None).localizedName;

        [PublicAPI("The government of the station, if the commander is docked")]
        public string stationgovernment => (controllingstationfaction?.Government ?? Government.None).localizedName;

        [PublicAPI("The super power allegiance of the station, if the commander is docked")]
        public string stationallegiance => (controllingstationfaction?.Allegiance ?? Superpower.None).localizedName;

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
            .Where( kvp => kvp.progress >= 30 )
            .Select( kvp => kvp.Power )
            .ToList() ?? new List<Power>();

        [PublicAPI( "The state of powerplay efforts within the star system, as an object" )]
        public PowerplayState PowerState { get; private set; }

        [PublicAPI( "The progress of nearby powerplay powers towards obtaining control of the star system, as a list of objects with keys Power (as an object) and progress (as a percent)" )]
        public List<PowerAcquisitionProgress> powerAcquisitionProgress { get; private set; }

        [PublicAPI( "The percent progress of the controlling power, if any, in consolidating control over the star system. Values below 0% indicate a reduction in the control level at the end of the cycle while values above 100% indicate an increase in the control level at the end of the cycle (if the current control state is less than 'Stronghold')" )]
        public decimal powerControlProgress { get; set; }

        [PublicAPI( "The control points accumulated by the controlling power via powerplay reinforcement activities during the current cycle" )]
        public int powerReinforcementControlPoints { get; set; }

        [PublicAPI( "The control points lost by the controlling power via powerplay undermining activities during the current cycle" )]
        public int powerUnderminingControlPoints { get; set; }

        // Deprecated, maintained for compatibility with user scripts
        [Obsolete("Use systemname instead")]
        public string system => systemname;

        [Obsolete("Use bodyname instead")]
        public string body => bodyname;

        [Obsolete("Use systemfaction instead")]
        public string faction => controllingsystemfaction?.name;

        [Obsolete("Use systemstate instead")]
        public string factionstate => (controllingsystemfaction?.presences.FirstOrDefault(p => p.systemAddress == systemAddress )?.FactionState ?? FactionState.None).localizedName;

        [Obsolete("Use systemgovernment instead")]
        public string government => (controllingsystemfaction?.Government ?? Government.None).localizedName;

        [Obsolete("Use systemallegiance instead")]
        public string allegiance => (controllingsystemfaction?.Allegiance ?? Superpower.None).localizedName;

        // Thargoid War
        [PublicAPI( "Thargoid war data, when applicable" )]
        public ThargoidWar ThargoidWar { get; private set; }

        // These properties are not intended to be user facing

        public Economy Economy { get; private set; }

        public Economy Economy2 { get; private set; }

        public List<EconomyShare> stationEconomies { get; private set; }

        public Faction controllingsystemfaction { get; private set; }

        public Faction controllingstationfaction { get; private set; }

        public SecurityLevel securityLevel { get; private set; }

        public StationModel stationModel { get; private set; }

        public List<StationService> stationServices { get; private set; }

        public BodyType bodyType { get; set; }

        public bool taxi { get; private set; }
        
        public bool multicrew { get; private set; }
        
        public bool inSRV { get; private set; }
        
        public bool onFoot { get; private set; }

        public LocationEvent ( DateTime timestamp, string systemName, ulong systemAddress, decimal x, decimal y, decimal z,
            decimal? distancefromstar, string bodyName, int? bodyId, BodyType bodytype, decimal? longitude, decimal? latitude,
            bool docked, string station, StationModel stationtype, long? marketId, List<StationService> stationServices,
            Faction systemFaction, Faction stationFaction, List<Faction> factions, List<Conflict> conflicts,
            List<EconomyShare> stationEconomies, Economy economy, Economy economy2, SecurityLevel security, long? population,
            Power controllingPower, List<Power> powerplayPowers, PowerplayState powerplayState,
            List<PowerAcquisitionProgress> powerAcquisitionProgress,
            decimal powerplayControlProgress, int powerplayReinforcementControlPoints,
            int powerplayUnderminingControlPoints, bool taxi, bool multicrew, bool inSRV,
            bool onFoot, ThargoidWar thargoidWar ) : base( timestamp, NAME )
        {
            this.systemname = systemName;
            this.x = x;
            this.y = y;
            this.z = z;
            this.systemAddress = systemAddress;
            this.distancefromstar = distancefromstar;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.bodyType = bodytype ?? BodyType.None;
            this.docked = docked;
            this.station = station;
            this.stationModel = stationtype ?? StationModel.None;
            this.stationServices = stationServices ?? new List<StationService>();
            this.marketId = marketId;
            this.controllingsystemfaction = systemFaction;
            this.controllingstationfaction = stationFaction;
            this.stationEconomies = stationEconomies ?? new List<EconomyShare>();
            this.Economy = economy ?? Economy.None;
            this.Economy2 = economy2 ?? Economy.None;
            this.securityLevel = security ?? SecurityLevel.None;
            this.population = population;
            this.longitude = longitude;
            this.latitude = latitude;
            this.factions = factions ?? new List<Faction>();
            this.conflicts = conflicts ?? new List<Conflict>();
            this.taxi = taxi;
            this.multicrew = multicrew;
            this.inSRV = inSRV;
            this.onFoot = onFoot;
            this.ThargoidWar = thargoidWar;

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

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var systemName = JsonParsing.getString(data, "StarSystem");

            if ( systemName == "Training" )
            {
                // Training system; ignore
                return true;
            }

            data.TryGetValue( "StarPos", out var val );
            var starPos = (List<object>)val;
            var x = Math.Round(JsonParsing.getDecimal("X", starPos?[0]) * 32) / (decimal)32.0;
            var y = Math.Round(JsonParsing.getDecimal("Y", starPos?[1]) * 32) / (decimal)32.0;
            var z = Math.Round(JsonParsing.getDecimal("Z", starPos?[2]) * 32) / (decimal)32.0;
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var distFromStarLs = JsonParsing.getOptionalDecimal(data, "DistFromStarLS");

            var body = JsonParsing.getString(data, "Body");
            var bodyId = JsonParsing.getOptionalInt(data, "BodyID");
            var bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType")) ?? BodyType.None;

            var docked = JsonParsing.getBool(data, "Docked");
            var economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy"));
            var economy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy"));
            var security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity"));
            var population = JsonParsing.getOptionalLong(data, "Population");

            // If docked
            var marketId = JsonParsing.getOptionalLong(data, "MarketID");
            string stationName = null;
            string stationLocalizedName = null;
            StationModel stationtype = null;
            if ( marketId != null )
            {
                EventParsing.StationNameAndType( data, out stationName, out stationLocalizedName, out stationtype );
            }

            // Get station services data
            data.TryGetValue( "StationServices", out val );
            var stationservices = (val as List<object>)?.Cast<string>()?.ToList() ?? new List<string>();
            var stationServices = new List<StationService>();
            foreach ( var service in stationservices )
            {
                stationServices.Add( StationService.FromEDName( service ) );
            }

            // Get station economies and their shares
            data.TryGetValue( "StationEconomies", out var val2 );
            var economies = val2 as List<object> ?? new List<object>();
            var Economies = new List<EconomyShare>();
            foreach ( var economyshare in economies.Cast<IDictionary<string, object>>() )
            {
                var economyShare = Economy.FromEDName(JsonParsing.getString(economyshare, "Name"));
                economyShare.fallbackLocalizedName = JsonParsing.getString( economyshare, "Name_Localised" );
                var share = JsonParsing.getDecimal(economyshare, "Proportion");
                if ( economyShare != Economy.None && share > 0 )
                {
                    Economies.Add( new EconomyShare( economyShare, share ) );
                }
            }

            // If landed
            var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
            var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

            // Parse factions array data
            var factions = new List<Faction>();
            data.TryGetValue( "Factions", out var factionsVal );
            if ( factionsVal != null )
            {
                factions = EventParsing.Factions( factionsVal, systemName, systemAddress );
            }
            var systemfaction = EventParsing.Faction(data, "System", systemName, systemAddress, factions);
            var stationfaction = EventParsing.Faction(data, "Station", systemName, systemAddress, factions);

            // Parse conflicts array data
            var conflicts = new List<Conflict>();
            data.TryGetValue( "Conflicts", out var conflictsVal );
            if ( conflictsVal != null )
            {
                conflicts = EventParsing.FactionConflicts( conflictsVal, factions );
            }

            // Powerplay data (if pledged)
            EventParsing.PowerplayDetails( data, systemAddress, out var controllingPower,
                out var powersInAcquisitionRange, out var powerplayState,
                out var powerAcquisitionProgress,
                out var powerplayControlProgress, out var powerplayReinforcementControlPoints,
                out var powerplayUnderminingControlPoints );

            var taxi = JsonParsing.getOptionalBool(data, "Taxi") ?? false;
            var multicrew = JsonParsing.getOptionalBool(data, "Multicrew") ?? false;
            var inSRV = JsonParsing.getOptionalBool(data, "InSRV") ?? false;
            var onFoot = JsonParsing.getOptionalBool(data, "OnFoot") ?? false;

            // Thargoid war data (if any)
            EventParsing.ThargoidWarData( data, out var thargoidWar );

            events.Add( new LocationEvent( timestamp, systemName, systemAddress, x, y, z,
                distFromStarLs, body, bodyId, bodyType, longitude, latitude, docked,
                stationLocalizedName ?? stationName, stationtype, marketId, stationServices, systemfaction,
                stationfaction, factions, conflicts, Economies, economy, economy2, security,
                population, controllingPower, powersInAcquisitionRange, powerplayState,
                powerAcquisitionProgress, powerplayControlProgress,
                powerplayReinforcementControlPoints, powerplayUnderminingControlPoints,
                taxi, multicrew, inSRV, onFoot, thargoidWar )
            {
                raw = line,
                fromLoad = fromLogLoad
            } );
            return true;
        }
    }
}