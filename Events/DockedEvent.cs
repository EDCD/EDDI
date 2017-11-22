using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DockedEvent : Event
    {
        public const string NAME = "Docked";
        public const string DESCRIPTION = "Triggered when your ship docks at a station or outpost";
        public const string SAMPLE = @"{ ""timestamp"":""2017-08-18T10:52:26Z"", ""event"":""Docked"", ""StationName"":""Goddard Hub"", ""StationType"":""Coriolis"", ""StarSystem"":""HR 3499"", ""StationFaction"":""Labour of HR 3499"", ""StationGovernment"":""$government_Democracy;"", ""StationGovernment_Localised"":""Democracy"", ""StationAllegiance"":""Federation"", ""StationServices"":[ ""Dock"", ""Autodock"", ""BlackMarket"", ""Commodities"", ""Contacts"", ""Exploration"", ""Missions"", ""Outfitting"", ""CrewLounge"", ""Rearm"", ""Refuel"", ""Repair"", ""Shipyard"", ""Tuning"", ""MissionsGenerated"", ""FlightController"", ""StationOperations"", ""Powerplay"", ""SearchAndRescue"" ], ""StationEconomy"":""$economy_Industrial;"", ""StationEconomy_Localised"":""Industrial"", ""DistFromStarLS"":129.454132 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockedEvent()
        {
            VARIABLES.Add("station", "The station at which the commander has docked");
            VARIABLES.Add("system", "The system at which the commander has docked");
            VARIABLES.Add("model", "The model of the station at which the commander has docked (Orbis, Coriolis, etc)");
            VARIABLES.Add("faction", "The faction controlling the station at which the commander has docked");
            VARIABLES.Add("factionstate", "The state of the faction controlling the station at which the commander has docked");
            VARIABLES.Add("LocalFactionState", "The translation of the state of the faction controlling the station into the chosen language");
            VARIABLES.Add("economy", "The economy of the station at which the commander has docked");
            VARIABLES.Add("LocalEconomy", "The translation of the economy type into the chosen language");
            VARIABLES.Add("government", "The government of the station at which the commander has docked");
            VARIABLES.Add("LocalGovernment", "The government of the station translated into the chosen language");
            VARIABLES.Add("security", "The security of the station at which the commander has docked");
            VARIABLES.Add("distancefromstar", "The distance of this station from the star (light seconds)");
            VARIABLES.Add("stationservices", "A list of possible station services: Dock, Autodock, BlackMarket, Commodities, Contacts, Exploration, Initiatives, Missions, Outfitting, CrewLounge, Rearm, Refuel, Repair, Shipyard, Tuning, Workshop, MissionsGenerated, Facilitator, Research, FlightController, StationOperations, OnDockMission, Powerplay, SearchAndRescue");
        }

        public string system { get; private set; }

        public string station { get; private set; }

        public string model { get; private set; }

        public string faction { get; private set; }

        public string factionstate { get; private set; }

        public string LocalFactionState
        {
            get
            {
                if (factionstate != null && factionstate != "")
                {
                    return State.FromName(factionstate).LocalName;
                }
                else return null;
            }
        }

        public string economy { get; private set; }

        public string LocalEconomy
        {
            get
            {
                if (economy != null && economy != "")
                {
                    return Economy.FromName(economy).LocalName;
                }
                else return null;
            }
        }

        public string government { get; private set; }

        public string LocalGovernment
        {
            get
            {
                if (government != null && government != "")
                {
                    return Government.FromName(government).LocalName;
                }
                else return null;
            }
        }

        public decimal? distancefromstar { get; private set; }

        public List<string> stationservices { get; private set; }

        public DockedEvent(DateTime timestamp, string system, string station, string model, string faction, State factionstate, Economy economy, Government government, decimal? distancefromstar, List<string> stationservices) : base(timestamp, NAME)
        {
            this.system = system;
            this.station = station;
            this.model = model;
            this.faction = faction;
            this.factionstate = (factionstate == null ? State.None.name : factionstate.name);
            this.economy = (economy == null ? Economy.None.name : economy.name);
            this.government = (government == null ? Government.None.name : government.name);
            this.distancefromstar = distancefromstar;
            this.stationservices = stationservices;
        }
    }
}
