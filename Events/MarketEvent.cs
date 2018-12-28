using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MarketEvent : Event
    {
        public const string NAME = "Market";
        public const string DESCRIPTION = "Triggered when the market.json file has been updated";
        public const string SAMPLE = @"{ ""timestamp"":""2018-11-19T01:14:18Z"", ""event"":""Docked"", ""StationName"":""Linnaeus Enterprise"", ""StationType"":""Coriolis"", ""StarSystem"":""BD+48 738"", ""SystemAddress"":908352033466, ""MarketID"":3226360576, ""StationFaction"":""Laniakea"", ""StationGovernment"":""$government_Cooperative;"", ""StationGovernment_Localised"":""Cooperative"", ""StationServices"":[ ""Dock"", ""Autodock"", ""BlackMarket"", ""Commodities"", ""Contacts"", ""Exploration"", ""Missions"", ""Outfitting"", ""CrewLounge"", ""Rearm"", ""Refuel"", ""Repair"", ""Shipyard"", ""Tuning"", ""Workshop"", ""MissionsGenerated"", ""FlightController"", ""StationOperations"", ""Powerplay"", ""SearchAndRescue"", ""MaterialTrader"", ""StationMenu"" ], ""StationEconomy"":""$economy_Extraction;"", ""StationEconomy_Localised"":""Extraction"", ""StationEconomies"":[ { ""Name"":""$economy_Extraction;"", ""Name_Localised"":""Extraction"", ""Proportion"":1.000000 } ], ""DistFromStarLS"":59.295441, ""ActiveFine"":true }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MarketEvent()
        {
        }

        public long marketId { get; private set; }
        public string station { get; private set; }
        public string system { get; private set; }
        public List<MarketInfo> items { get; private set; }

        public MarketEvent(DateTime timestamp, long marketId, string station, string system, List<MarketInfo> items) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.station = station;
            this.system = system;
            this.items = items;
        }
    }
}
