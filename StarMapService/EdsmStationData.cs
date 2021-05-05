﻿using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        public List<Station> GetStarMapStations(string system, long? edsmId = null)
        {
            if (system == null) { return new List<Station>(); }

            var request = new RestRequest("api-system-v1/stations", Method.POST);
            request.AddParameter("systemName", system);
            request.AddParameter("systemId", edsmId);
            var clientResponse = restClient.Execute<JObject>(request);
            if (clientResponse.IsSuccessful)
            {
                var token = JToken.Parse(clientResponse.Content);
                if (token is JObject response)
                {
                    return ParseStarMapStationsParallel(response);
                }
            }
            else
            {
                Logging.Debug("EDSM responded with " + clientResponse.ErrorMessage, clientResponse.ErrorException);
            }
            return new List<Station>();
        }

        public List<Station> ParseStarMapStationsParallel(JObject response)
        {
            List<Station> Stations = new List<Station>();
            if (response != null)
            {
                string system = (string)response["name"];
                JArray stations = (JArray)response["stations"];

                if (stations != null)
                {
                    Stations = stations
                        .AsParallel()
                        .Select(s => ParseStarMapStation(s.ToObject<JObject>(), system))
                        .Where(s => s != null)
                        .ToList();
                }
                // Sort stations by distance 
                Stations = Stations.OrderBy(o => o.distancefromstar).ToList();
            }
            return Stations;
        }

        private Station ParseStarMapStation(JObject station, string system)
        {
            try
            {
                Station Station = new Station
                {
                    systemname = system,
                    name = (string)station["name"],
                    marketId = (long?)station["marketId"],
                    EDSMID = (long?)station["id"],
                    Model = StationModel.FromName((string)station["type"]) ?? StationModel.None,
                    distancefromstar = (decimal?)station["distanceToArrival"], // Light seconds
                };

                var faction = station["controllingFaction"]?.ToObject<Dictionary<string, object>>();
                Station.Faction = new Faction()
                {
                    name = (string)faction?["name"] ?? string.Empty,
                    EDSMID = (long?)faction?["id"],
                    Allegiance = Superpower.FromName((string)station["allegiance"]) ?? Superpower.None,
                    Government = Government.FromName((string)station["government"]) ?? Government.None,
                };

                List<EconomyShare> economyShares = new List<EconomyShare>()
            {
                { new EconomyShare(Economy.FromName((string)station["economy"]) ?? Economy.None, 0) },
                { new EconomyShare(Economy.FromName((string)station["secondEconomy"]) ?? Economy.None, 0) }
            };
                Station.economyShares = economyShares;

                List<StationService> stationServices = new List<StationService>();
                if ((bool?)station["haveMarket"] is true)
                {
                    stationServices.Add(StationService.FromEDName("Commodities"));
                };
                if ((bool?)station["haveShipyard"] is true)
                {
                    stationServices.Add(StationService.FromEDName("Shipyard"));
                };
                if ((bool?)station["haveOutfitting"] is true)
                {
                    stationServices.Add(StationService.FromEDName("Outfitting"));
                };
                var services = station["otherServices"].ToObject<List<string>>();
                foreach (string service in services)
                {
                    stationServices.Add(StationService.FromName(service));
                };
                // Add always available services for dockable stations
                stationServices.Add(StationService.FromEDName("Dock"));
                stationServices.Add(StationService.FromEDName("AutoDock"));
                stationServices.Add(StationService.FromEDName("Exploration"));
                stationServices.Add(StationService.FromEDName("Workshop"));
                stationServices.Add(StationService.FromEDName("FlightController"));
                stationServices.Add(StationService.FromEDName("StationOperations"));
                stationServices.Add(StationService.FromEDName("Powerplay"));
                stationServices.Add(StationService.FromEDName("SocialSpace"));
                Station.stationServices = stationServices;

                var updateTimes = station["updateTime"].ToObject<Dictionary<string, object>>();
                string datetime;
                datetime = JsonParsing.getString(updateTimes, "information");
                long? infoLastUpdated = Dates.fromDateTimeStringToSeconds(datetime);
                datetime = JsonParsing.getString(updateTimes, "market");
                long? marketLastUpdated = Dates.fromDateTimeStringToSeconds(datetime);
                datetime = JsonParsing.getString(updateTimes, "shipyard");
                long? shipyardLastUpdated = Dates.fromDateTimeStringToSeconds(datetime);
                datetime = JsonParsing.getString(updateTimes, "outfitting");
                long? outfittingLastUpdated = Dates.fromDateTimeStringToSeconds(datetime);
                List<long?> updatedAt = new List<long?>() { infoLastUpdated, marketLastUpdated, shipyardLastUpdated, outfittingLastUpdated };
                Station.updatedat = updatedAt.Max();
                Station.outfittingupdatedat = outfittingLastUpdated;
                Station.commoditiesupdatedat = marketLastUpdated;
                Station.shipyardupdatedat = shipyardLastUpdated;

                return Station;
            }
            catch (Exception ex)
            {
                Dictionary<string, object> data = new Dictionary<string, object>
                            {
                                {"station", JsonConvert.SerializeObject(station)},
                                {"exception", ex.Message},
                                {"stacktrace", ex.StackTrace}
                            };
                Logging.Error("Error parsing EDSM station result.", data);
            }
            return null;
        }
    }
}
