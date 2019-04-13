using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiEvents
{
    public class DockedEvent : Event
    {
        public const string NAME = "Docked";
        public const string DESCRIPTION = "Triggered when your ship docks at a station or outpost";
        public const string SAMPLE = @"{ ""timestamp"":""2018-11-19T01:14:18Z"", ""event"":""Docked"", ""StationName"":""Linnaeus Enterprise"", ""StationType"":""Coriolis"", ""StarSystem"":""BD+48 738"", ""SystemAddress"":908352033466, ""MarketID"":3226360576, ""StationFaction"":""Laniakea"", ""StationGovernment"":""$government_Cooperative;"", ""StationGovernment_Localised"":""Cooperative"", ""StationServices"":[ ""Dock"", ""Autodock"", ""BlackMarket"", ""Commodities"", ""Contacts"", ""Exploration"", ""Missions"", ""Outfitting"", ""CrewLounge"", ""Rearm"", ""Refuel"", ""Repair"", ""Shipyard"", ""Tuning"", ""Workshop"", ""MissionsGenerated"", ""FlightController"", ""StationOperations"", ""Powerplay"", ""SearchAndRescue"", ""MaterialTrader"", ""StationMenu"" ], ""StationEconomy"":""$economy_Extraction;"", ""StationEconomy_Localised"":""Extraction"", ""StationEconomies"":[ { ""Name"":""$economy_Extraction;"", ""Name_Localised"":""Extraction"", ""Proportion"":1.000000 } ], ""DistFromStarLS"":59.295441, ""ActiveFine"":true }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockedEvent()
        {
            VARIABLES.Add("station", "The station at which the commander has docked");
            VARIABLES.Add("marketId", "The market ID of station at which the commander has docked");
            VARIABLES.Add("system", "The system at which the commander has docked");
            VARIABLES.Add("state", "The special state of the station, if applicable (\"Damaged\" for damaged stations for example)");
            VARIABLES.Add("model", "The model of the station at which the commander has docked (Orbis, Coriolis, etc)");
            VARIABLES.Add("allegiance", "The superpower allegiance of the station at which the commander has docked");
            VARIABLES.Add("faction", "The faction controlling the station at which the commander has docked");
            VARIABLES.Add("factionstate", "The state of the faction controlling the station at which the commander has docked");
            VARIABLES.Add("economy", "The economy of the station at which the commander has docked");
            VARIABLES.Add("secondeconomy", "The secondary economy of the station at which the commander has docked");
            VARIABLES.Add("government", "The government of the station at which the commander has docked");
            VARIABLES.Add("security", "The security of the station at which the commander has docked");
            VARIABLES.Add("distancefromstar", "The distance of this station from the star (light seconds)");
            VARIABLES.Add("stationservices", "A list of possible station services: Dock, Autodock, BlackMarket, Commodities, Contacts, Exploration, Initiatives, Missions, Outfitting, CrewLounge, Rearm, Refuel, Repair, Shipyard, Tuning, Workshop, MissionsGenerated, Facilitator, Research, FlightController, StationOperations, OnDockMission, Powerplay, SearchAndRescue, TechBroker, MaterialTrader");
            VARIABLES.Add("cockpitbreach", "True if landing with a breached cockpit");
            VARIABLES.Add("wanted", "True if landing at a station where you are wanted");
            VARIABLES.Add("activefine", "True if landing at a station where you have active fines");
        }

        public string system { get; private set; }

        public string station { get; private set; }

        public long marketId { get; private set; }

        public string state { get; private set; }

        public string model => stationModel.localizedName;

        public string economy => economyShares.Count > 0 ? (economyShares[0]?.economy ?? Economy.None).localizedName : Economy.None.localizedName;

        public string secondeconomy => economyShares.Count > 1 ? (economyShares[1]?.economy ?? Economy.None).localizedName : Economy.None.localizedName;

        public decimal? distancefromstar { get; private set; }

        public List<string> stationservices
        {
            get
            {
                List<string> services = new List<string>();
                foreach (StationService service in stationServices)
                {
                    services.Add(service.localizedName);
                }
                return services;
            }
        }

        public bool cockpitbreach { get; private set; }
        public bool wanted { get; private set; }
        public bool activefine { get; private set; }

        // Faction properties
        public string faction => controllingfaction?.name;
        public string factionstate => (controllingfaction?.presences.FirstOrDefault(p => p.systemName == system)?.FactionState ?? FactionState.None).localizedName;
        public string allegiance => (controllingfaction?.Allegiance ?? Superpower.None).localizedName;
        public string government => (controllingfaction?.Government ?? Government.None).localizedName;

        // These properties are not intended to be user facing
        public long systemAddress { get; private set; }
        public StationModel stationModel { get; private set; } = StationModel.None;
        public Faction controllingfaction { get; private set; }
        public List<StationService> stationServices { get; private set; } = new List<StationService>();
        public FactionState factionState { get; private set; } = FactionState.None;

        public List<EconomyShare> economyShares { get; private set; } = new List<EconomyShare>() { new EconomyShare(Economy.None, 0M), new EconomyShare(Economy.None, 0M) };

        public DockedEvent(DateTime timestamp, string system, long systemAddress, long marketId, string station, string state, StationModel stationModel, Faction controllingfaction, List<EconomyShare> Economies, decimal? distancefromstar, List<StationService> stationServices, bool cockpitBreach, bool wanted, bool activeFine) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.marketId = marketId;
            this.station = station;
            this.state = state;
            this.stationModel = stationModel ?? StationModel.None;
            this.controllingfaction = controllingfaction;
            this.economyShares = Economies;
            this.distancefromstar = distancefromstar;
            this.stationServices = stationServices;
            this.cockpitbreach = cockpitBreach;
            this.wanted = wanted;
            this.activefine = activeFine;
        }
    }
}