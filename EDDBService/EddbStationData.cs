using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEddbService
{
    partial class EddbService
    {
        /// <summary> At least one station name is required. </summary>
        public static List<Station> Stations(string[] stationNames)
        {
            List<Station> stations = new List<Station>();
            foreach (string name in stationNames)
            {
                Station station = GetStation(new KeyValuePair<string, object>(StationQuery.stationName, name)) ?? new Station();
                if (station.EDDBID == null)
                {
                    station.name = name;
                }
                stations.Add(station);
            }
            return stations?.OrderBy(x => x.name).ToList();
        }

        /// <summary> At least one station EDDBID is required. </summary>
        public static List<Station> Stations(long[] eddbStationIds)
        {
            List<Station> stations = new List<Station>();
            foreach (long eddbId in eddbStationIds)
            {
                Station station = GetStation(new KeyValuePair<string, object>(StationQuery.eddbId, eddbId)) ?? new Station();
                if (station.EDDBID == null)
                {
                    station.EDDBID = eddbId;
                }
                stations.Add(station);
            }
            return stations?.OrderBy(x => x.name).ToList();
        }

        /// <summary> Exactly one system name is required. </summary>
        public static List<Station> Stations(string systemName)
        {
            return GetStations(new KeyValuePair<string, object>(StationQuery.systemName, systemName))
                ?.OrderBy(x => x.distancefromstar).ToList() ?? new List<Station>();
        }

        /// <summary> Exactly one station name is required. </summary>
        public static Station Station(string stationName)
        {
            Station station = GetStation(new KeyValuePair<string, object>(StationQuery.stationName, stationName)) ?? new Station();
            if (station.EDDBID == null)
            {
                station.name = stationName;
            }
            return station;
        }

        /// <summary> Exactly one station name and system name are required. </summary>
        public static Station Station(string stationName, string systemName)
        {
            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>();
            queryList.Add(new KeyValuePair<string, object>(StationQuery.stationName, stationName));
            queryList.Add(new KeyValuePair<string, object>(StationQuery.systemName, systemName));
            return GetStation(queryList);
        }

        /// <summary> Exactly one station EDDBID is required. </summary>
        public static Station Station(long eddbId)
        {
            return GetStation(new KeyValuePair<string, object>(StationQuery.eddbId, eddbId));
        }

        private static Station GetStation(KeyValuePair<string, object> query)
        {
            if (query.Value != null)
            {
                List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>();
                queryList.Add(query);

                List<object> responses = GetData(Endpoint.stations, queryList);

                if (responses != null)
                {
                    return ParseEddbStation(responses[0]);
                }
            }
            return null;
        }

        private static Station GetStation(List<KeyValuePair<string, object>> queryList)
        {
            if (queryList.Count > 0)
            {
                List<object> responses = GetData(Endpoint.stations, queryList);

                if (responses != null)
                {
                    return ParseEddbStation(responses[0]);
                }
            }
            return null;
        }

        private static List<Station> GetStations(KeyValuePair<string, object> query)
        {
            if (query.Value != null)
            {
                List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>();
                queryList.Add(query);

                List<object> responses = GetData(Endpoint.stations, queryList);

                if (responses != null)
                {
                    List<Station> stations = new List<Station>();
                    foreach (object response in responses)
                    {
                        Station station = ParseEddbStation(response);
                        if (station != null) { stations.Add(station); };
                    }
                    return stations;
                }
            }
            return null;
        }

        private static Station ParseEddbStation(object response)
        {
            // Notes:
            // Whether a station is planetary is not given by the journal. Though EDDB provides this as a boolean, 
            //   we will derive this from the station model instead of using the EDDB value to maximize consistency with journal data.
            // Economy share / proportion is not given from EDDB, so we cannot use the EconomyShare class 
            //   to describe these economies and we fall back to strings.
            // CommodityMarketQuote data is not currently available from EDDB. Pilots will have to dock to retrieve this.

            JObject stationJson = ((JObject)response);

            Station Station = new Station();

            // General data
            Station.EDDBID = (long)stationJson["id"];
            Station.name = (string)stationJson["name"];
            Station.bodyEDDBID = (long?)stationJson["body_id"];
            Station.distancefromstar = (long?)stationJson["distance_to_star"]; // Light seconds

            // System name is not given directly, but we have an EDDB ID linking to the system 
            // that we will be able to use for lookups once our system lookup is complete.
            // We will also need to use this lookup to find the reserve level associated with this system
            Station.systemEDDBID = (long?)stationJson["system_id"];
            /*
            Station.systemname = systemName;
            */

            // Controlling faction data
            long? factionEddbId = (long?)stationJson["controlling_minor_faction_id"];
            Faction controllingFaction;
            if (factionEddbId != null)
            {
                controllingFaction = Faction((long)factionEddbId);
            }
            else
            {
                controllingFaction = new Faction();
            };
            controllingFaction.Government = Government.FromName((string)stationJson["government"]);
            controllingFaction.Allegiance = Superpower.FromName((string)stationJson["allegiance"]);
            Station.Faction = controllingFaction;

            // The local state may or may not match the faction's general state, so we set it here
            Station.State = State.FromName((string)stationJson["state"] == "None" ? null : (string)stationJson["state"]);

            // Station facilities data
            Station.Model = StationModel.FromName(eddbModel2journalModel((string)stationJson["type"]));
            Station.LargestPad = StationLargestPad.FromSize((string)stationJson["max_landing_pad_size"]);
            Station.hasblackmarket = (bool?)stationJson["has_blackmarket"];
            Station.hasdocking = (bool?)stationJson["has_docking"];
            Station.hasmarket = (bool?)stationJson["has_market"];
            Station.hasoutfitting = (bool?)stationJson["has_outfitting"];
            Station.hasrefuel = (bool?)stationJson["has_refuel"];
            Station.hasrepair = (bool?)stationJson["has_repair"];
            Station.hasrearm = (bool?)stationJson["has_rearm"];
            Station.hasshipyard = (bool?)stationJson["has_shipyard"];

            List<string> economies = new List<string>();
            if (stationJson["economies"] != null)
            {
                foreach (JObject export in stationJson["economies"])
                {
                    string economy = (string)export["name"];
                    economies.Add(economy);
                }
            }
            Station.economies = economies;

            List<string> imports = new List<string>();
            if (stationJson["import_commodities"] != null)
            {
                foreach (JObject import in stationJson["import_commodities"])
                {
                    string name = (string)import["name"];
                    imports.Add(name);
                }
            }
            Station.imported = imports;

            List<string> exports = new List<string>();
            if (stationJson["export_commodities"] != null)
            {
                foreach (JObject export in stationJson["export_commodities"])
                {
                    string name = (string)export["name"];
                    exports.Add(name);
                }
            }
            Station.exported = exports;

            List<string> prohibited = new List<string>();
            if (stationJson["prohibited_commodities"] != null)
            {
                foreach (JObject export in stationJson["prohibited_commodities"])
                {
                    string name = (string)export["name"];
                    prohibited.Add(name);
                }
            }
            Station.prohibited = prohibited;

            List<Ship> shipyard = new List<Ship>();
            if (stationJson["selling_ships"] != null)
            {
                foreach (JObject shipJson in stationJson["selling_ships"])
                {
                    Ship ship = ShipDefinitions.FromModel((string)shipJson["name"]);
                    shipyard.Add(ship);
                }
            }
            Station.shipyard = shipyard;

            List<Module> outfitting = new List<Module>();
            if (stationJson["selling_modules"] != null)
            {
                JArray modulesJArray = (JArray)stationJson["selling_modules"];
                foreach (JValue eddbIdValue in modulesJArray)
                {
                    Module module = Module.FromEddbID((long)eddbIdValue);
                    outfitting.Add(module);
                }
            }
            Station.outfitting = outfitting;

            Station.outfittingupdatedat = Dates.fromDateTimeStringToSeconds((string)stationJson["outfitting_updated_at"]);
            Station.shipyardupdatedat = Dates.fromDateTimeStringToSeconds((string)stationJson["shipyard_updated_at"]);
            Station.updatedat = Dates.fromDateTimeStringToSeconds((string)stationJson["updated_at"]);

            return Station;
        }

        private static string eddbModel2journalModel(string eddbModelName)
        {
            if (eddbModelName == null)
            {
                return null;
            }

            eddbModelName = eddbModelName.ToLowerInvariant();

            if (eddbModelName.StartsWith("planetary") || eddbModelName.EndsWith("planetary"))
            {
                return "surface station";
            }

            if (eddbModelName.EndsWith("starport"))
            {
                eddbModelName.Replace(" starport", "");

                // Ocellus starports are described by the journal as "Bernal" so we make the replacement here.
                eddbModelName.Replace("ocellus", "bernal");

                return eddbModelName;
            }

            if (eddbModelName.EndsWith("outpost"))
            {
                // Though EDDB provides details on the type of outpost (military, civilian, scientific, etc.), 
                // that info is not present in the journal. Strip it for consistent output between journal and EDDB data.
                return "outpost";
            }

            return eddbModelName;
        }
    }
}
