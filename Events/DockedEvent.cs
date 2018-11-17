using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DockedEvent : Event
    {
        public const string NAME = "Docked";
        public const string DESCRIPTION = "Triggered when your ship docks at a station or outpost";
        public const string SAMPLE = @"{ ""timestamp"":""2017-08-18T10:52:26Z"", ""event"":""Docked"", ""StationName"":""Goddard Hub"", ""StationType"":""Coriolis"", ""MarketID"": 3229040640, ""StarSystem"":""HR 3499"", ""SystemAddress"": 1831543523683, ""StationFaction"":""Labour of HR 3499"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationAllegiance"":""Federation"", ""StationServices"":[ ""Dock"", ""Autodock"", ""BlackMarket"", ""Commodities"", ""Contacts"", ""Exploration"", ""Missions"", ""Outfitting"", ""CrewLounge"", ""Rearm"", ""Refuel"", ""Repair"", ""Shipyard"", ""Tuning"", ""MissionsGenerated"", ""FlightController"", ""StationOperations"", ""Powerplay"", ""SearchAndRescue"" ], ""StationEconomy"":""$economy_Industrial;"", ""StationEconomy_Localised"":""Industrial"", ""DistFromStarLS"":129.454132 }";

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
        }

        public string system { get; private set; }

        public string station { get; private set; }

        public long marketId { get; private set; }

        public string state { get; private set; }

        public string model => stationModel.localizedName;

        public string allegiance => Allegiance.localizedName;

        public string faction { get; private set; }

        public string factionstate => factionState.localizedName;

        public string economy => economyShares.Count > 0 ? (economyShares[0]?.economy ?? Economy.None).localizedName : Economy.None.localizedName;

        public string secondeconomy => economyShares.Count > 1 ? (economyShares[1]?.economy ?? Economy.None).localizedName : Economy.None.localizedName;

        public string government => Government.localizedName;

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

        // These properties are not intended to be user facing
        public long systemAddress { get; private set; }
        public StationModel stationModel { get; private set; } = StationModel.None;
        public Superpower Allegiance { get; private set; } = Superpower.None;
        public List<StationService> stationServices { get; private set; } = new List<StationService>();
        public FactionState factionState { get; private set; } = FactionState.None;
        public Government Government { get; private set; } = Government.None;
        public List<EconomyShare> economyShares { get; private set; } = new List<EconomyShare>() { new EconomyShare(Economy.None, 0M), new EconomyShare(Economy.None, 0M) };

        public DockedEvent(DateTime timestamp, string system, long systemAddress, long marketId, string station, string state, StationModel stationModel, Superpower Allegiance, string faction, FactionState factionState, List<EconomyShare> Economies, Government Government, decimal? distancefromstar, List<StationService> stationServices) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.marketId = marketId;
            this.station = station;
            this.state = state;
            this.stationModel = stationModel ?? StationModel.None;
            this.Allegiance = Allegiance ?? Superpower.None;
            this.faction = faction;
            this.factionState = factionState ?? FactionState.None;
            this.economyShares = Economies;
            this.Government = Government ?? Government.None;
            this.distancefromstar = distancefromstar;
            this.stationServices = stationServices;
        }
    }
}