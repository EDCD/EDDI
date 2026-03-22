using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DockedEvent ( DateTime timestamp, Station station, bool cockpitBreach, bool wanted, bool activeFine )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Docked";
        public const string DESCRIPTION = "Triggered when your ship docks at a station or outpost";
        public const string SAMPLE = @"{ ""timestamp"":""2018-11-19T01:14:18Z"", ""event"":""Docked"", ""StationName"":""Linnaeus Enterprise"", ""StationType"":""Coriolis"", ""StarSystem"":""BD+48 738"", ""SystemAddress"":908352033466, ""MarketID"":3226360576, ""StationFaction"":""Laniakea"", ""StationGovernment"":""$government_Cooperative;"", ""StationGovernment_Localised"":""Cooperative"", ""StationServices"":[ ""Dock"", ""Autodock"", ""BlackMarket"", ""Commodities"", ""Contacts"", ""Exploration"", ""Missions"", ""Outfitting"", ""CrewLounge"", ""Rearm"", ""Refuel"", ""Repair"", ""Shipyard"", ""Tuning"", ""Workshop"", ""MissionsGenerated"", ""FlightController"", ""StationOperations"", ""Powerplay"", ""SearchAndRescue"", ""MaterialTrader"", ""StationMenu"" ], ""StationEconomy"":""$economy_Extraction;"", ""StationEconomy_Localised"":""Extraction"", ""StationEconomies"":[ { ""Name"":""$economy_Extraction;"", ""Name_Localised"":""Extraction"", ""Proportion"":1.000000 } ], ""DistFromStarLS"":59.295441, ""ActiveFine"":true, ""LandingPads"": {""Large"": 6, ""Medium"": 13, ""Small"": 9 } }";

        [ PublicAPI( "The system at which the commander has docked" ) ]
        public string system => Station?.systemname;

        [PublicAPI( "The numeric system address of the star system where the commander has docked" )]
        public ulong systemAddress => Station?.systemAddress ?? 0;

        [ PublicAPI( "The station at which the commander has docked" ) ]
        public string station => Station?.name;

        [ PublicAPI( "The numeric market ID of station at which the commander has docked" ) ]
        public long? marketId => Station?.marketId;

        [ PublicAPI( "The state of the station (e.g. \"Abandoned\", \"Damaged\", \"Normal Operation\", \"Under Attack\", \"Under Repairs\")" ) ]
        public string state => Station?.stationState.localizedName;

        [PublicAPI("The model of the station at which the commander has docked (Orbis, Coriolis, etc)")]
        public string model => Station?.Model.localizedName;

        [PublicAPI("The economy of the station at which the commander has docked")]
        public string economy => Station?.economyShares.Count > 0 ? ( Station?.economyShares[0].economy ?? Economy.None).localizedName : Economy.None.localizedName;

        [PublicAPI("The secondary economy of the station at which the commander has docked")]
        public string secondeconomy => Station?.economyShares.Count > 1 ? ( Station?.economyShares[1].economy ?? Economy.None).localizedName : Economy.None.localizedName;

        [PublicAPI("The distance of this station from the star (light seconds)")]
        public decimal? distancefromstar => Station?.distancefromstar;

        [ PublicAPI("A list of possible station services: Dock, Autodock, BlackMarket, Commodities, Contacts, Exploration, Initiatives, Missions, Outfitting, CrewLounge, Rearm, Refuel, Repair, Shipyard, Tuning, Workshop, MissionsGenerated, Facilitator, Research, FlightController, StationOperations, OnDockMission, Powerplay, SearchAndRescue, TechBroker, MaterialTrader")]
        public List<string> stationservices => Station?.stationServices.Select( s => s.localizedName ).ToList();

        [PublicAPI("True if landing with a breached cockpit")]
        public bool cockpitbreach { get; private set; } = cockpitBreach;

        [PublicAPI("True if landing at a station where you are wanted")]
        public bool wanted { get; private set; } = wanted;

        [PublicAPI("True if landing at a station where you have active fines")]
        public bool activefine { get; private set; } = activeFine;

        // Faction properties

        [PublicAPI("The faction controlling the station at which the commander has docked")]
        public string faction => Station?.Faction?.name;

        [PublicAPI("The state of the faction controlling the station at which the commander has docked")]
        public string factionstate => ( Station?.Faction?.presences.FirstOrDefault(p => p.systemAddress == systemAddress )?.FactionState ?? FactionState.None).localizedName;
       
        [PublicAPI("The superpower allegiance of the station at which the commander has docked")]
        public string allegiance => Station?.Faction?.Allegiance.localizedName;

        [PublicAPI("The government of the station at which the commander has docked")]
        public string government => Station?.Faction?.Government.localizedName;

        // These properties are not intended to be user facing

        public StationModel stationModel => Station?.Model;

        public StationState stationState => Station?.stationState;

        public Faction controllingfaction => Station?.Faction;

        public List<EconomyShare> economyShares => Station?.economyShares;

        public StationLandingPads landingPads => Station?.landingPads;

        public Station Station { get; private set; } = station;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, List<Faction> factions, ref List<Event> events, bool fromLogLoad )
        {
            var systemName = JsonParsing.getString(data, "StarSystem");
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var marketId = JsonParsing.getOptionalLong(data, "MarketID");
            EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationModel );

            var controllingfaction = EventParsing.Faction(data, "Station", systemName, systemAddress, factions);
            var distancefromstar = JsonParsing.getOptionalDecimal(data, "DistFromStarLS");

            // Get station landing pads data
            var landingPads = EventParsing.LandingPadsCollection(data);

            // Get station services data
            data.TryGetValue( "StationServices", out var val );
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
                var economy = Economy.FromEDName(JsonParsing.getString(economyshare, "Name"));
                economy.fallbackLocalizedName = JsonParsing.getString( economyshare, "Name_Localised" );
                var share = JsonParsing.getDecimal(economyshare, "Proportion");
                if ( economy != Economy.None && share > 0 )
                {
                    Economies.Add( new EconomyShare( economy, share ) );
                }
            }

            var cockpitBreach = JsonParsing.getOptionalBool(data, "CockpitBreach") ?? false;
            var wanted = JsonParsing.getOptionalBool(data, "Wanted") ?? false;
            var activeFine = JsonParsing.getOptionalBool(data, "ActiveFine") ?? false;

            var stationStateEdName = JsonParsing.getString( data, "StationState" );
            var stationState = string.IsNullOrEmpty( stationStateEdName )
                                        ? StationState.NormalOperation
                                        : StationState.FromEDName( stationStateEdName );

            var station = new Station()
            {
                name = stationName,
                localizedName = stationLocalizedName,
                marketId = marketId,
                systemname = systemName,
                systemAddress = systemAddress,
                Model = stationModel,
                Faction = controllingfaction,
                stationServices = stationServices,
                stationState = stationState,
                economyShares = Economies,
                distancefromstar = distancefromstar,
                landingPads = landingPads
            };

            events.Add( new DockedEvent( timestamp, station, cockpitBreach, wanted, activeFine ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}