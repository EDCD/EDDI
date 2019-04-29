﻿using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Utilities;

namespace EddiDataProviderService
{
    /// <summary>Access to EDDP legacy server data<summary>
    public class LegacyEddpService
    {
        private const string BASE = "http://api.eddp.co:8080/";

        static LegacyEddpService()
        {
            // We need to not use an expect header as it causes problems when sending data to a REST service
            var errorUri = new Uri(BASE + "error");
            var errorServicePoint = ServicePointManager.FindServicePoint(errorUri);
            errorServicePoint.Expect100Continue = false;
        }

        public static StarSystem SetLegacyData(StarSystem system, bool setPowerplayData = true, bool setBodyData = true, bool setStationData = true)
        {
            JObject response = GetData(system.systemname);
            if (response != null)
            {
                SetStarSystemLegacyData(system, response, setPowerplayData);
                if (setBodyData) { SetBodyLegacyData(system, response); }
                if (setStationData) { SetStationLegacyData(system, response); }
            }
            return system;
        }

        public static JObject GetData(string system)
        {
            try
            {
                string response = Net.DownloadString(BASE + "systems/" + Uri.EscapeDataString(system));
                return response == null ? null : JsonConvert.DeserializeObject<JObject>(response);
            }
            catch (Exception)
            {
                Logging.Debug("Failed to obtain data from " + BASE + "systems/" + Uri.EscapeDataString(system));
                return null;
            }
        }

        private static void SetStarSystemLegacyData(StarSystem system, JObject json, bool setPowerplayData)
        {
            // Set data not currently available from EDSM: Powerplay data and EDDBID
            // TODO: Translatable powerplay states
            system.EDDBID = (long?)json["id"];
            if (setPowerplayData)
            {
                system.power = (string)json["power"] == "None" ? null : (string)json["power"];
                system.powerstate = (string)json["power_state"] == "None" ? null : (string)json["power_state"];
            }
        }

        private static void SetBodyLegacyData(StarSystem system, JObject response)
        {
            // Set data not currently available from EDSM: EDDBID
            if (response["bodies"] is JArray)
            {
                foreach (Body body in system.bodies.ToList())
                {
                    JObject Body = response["bodies"].Children<JObject>()
                        .FirstOrDefault(o => o["name"] != null && o["name"].ToString() == body.bodyname);

                    if (Body != null)
                    {
                        body.EDDBID = (long?)Body["id"];
                        body.systemEDDBID = system.EDDBID;
                    }
                }
            }
        }

        private static void SetStationLegacyData(StarSystem system, JObject response)
        {
            // Set data not currently available from EDSM
            if (response["stations"] is JArray)
            {
                foreach (Station station in system.stations.ToList())
                {
                    JObject Station = response["stations"].Children<JObject>()
                        .FirstOrDefault(o => o["name"] != null && o["name"].ToString() == station.name);

                    if (Station != null)
                    {
                        // Station EDDBID
                        station.EDDBID = (long?)Station["id"];

                        // Commodities price listings
                        station.commodities = CommodityQuotesFromEDDP(Station);
                        station.commoditiesupdatedat = (long?)Station["market_updated_at"];

                        // Detailed shipyard data 
                        List<Ship> shipyard = ShipyardFromEDDP(Station);
                        station.shipyard = shipyard;
                        station.shipyardupdatedat = (long?)Station["shipyard_updated_at"];

                        // Detailed module data
                        List<Module> modules = ModulesFromEDDP(Station);
                        station.outfitting = modules;
                        station.outfittingupdatedat = (long?)Station["outfitting_updated_at"];
                    }
                }
                SetPlanetarySettlementData(system, response); // Non-landable planetary settlement data
            }
        }

        private static List<CommodityMarketQuote> CommodityQuotesFromEDDP(JObject json)
        {
            var quotes = new List<CommodityMarketQuote>();
            if (json["commodities"] != null)
            {
                foreach (JObject commodity in json["commodities"])
                {
                    CommodityDefinition commodityDefinition = CommodityDefinition.FromNameOrEDName((string)commodity["name"]);
                    CommodityMarketQuote quote = new CommodityMarketQuote(commodityDefinition);
                    // Annoyingly, these double-casts seem to be necessary because the boxed type is `long`. 
                    // A direct cast to `int?` always returns null.
                    quote.buyprice = (int?)(long?)commodity["buy_price"] ?? quote.buyprice;
                    quote.sellprice = (int?)(long?)commodity["sell_price"] ?? quote.sellprice;
                    quote.demand = (int?)(long?)commodity["demand"] ?? quote.demand;
                    quote.stock = (int?)(long?)commodity["supply"] ?? quote.stock;
                    quotes.Add(quote);
                }
            }
            return quotes;
        }

        private static List<Ship> ShipyardFromEDDP(JObject Station)
        {
            List<string> sellingShips = (Station["selling_ships"]).ToObject<List<string>>();
            List<Ship> shipyard = new List<Ship>();
            foreach (string sellingShip in sellingShips)
            {
                Ship ship = ShipDefinitions.FromModel(sellingShip);
                if (ship != null) { shipyard.Add(ship); }
            }
            return shipyard;
        }

        private static List<Module> ModulesFromEDDP(JObject Station)
        {
            List<object> sellingModules = (Station["selling_modules"]).ToObject<List<object>>();
            List<Module> modules = new List<Module>();
            foreach (object sellingModule in sellingModules)
            {
                Module module = new Module();
                if (sellingModule is long)
                {
                    module = Module.FromEddbID((long)sellingModule);
                }
                else if (sellingModule is string)
                {
                    module = Module.FromEDName((string)sellingModule);
                }
                if (module != null) { modules.Add(module); }
            }
            return modules;
        }

        public static void SetPlanetarySettlementData(StarSystem system, JObject response)
        {
            if (response["stations"] is JArray)
            {
                foreach (JObject Station in response["stations"])
                {
                    if ((bool)Station["has_docking"] is false)
                    {
                        Station settlement = new Station
                        {
                            name = (string)Station["name"],
                            EDDBID = (long)Station["id"],
                            systemname = system.systemname,
                            hasdocking = false,
                            Model = StationModel.FromName((string)Station["type"]) ?? StationModel.None,
                            Economies = new List<Economy> { Economy.FromName((string)Station["primary_economy"]), Economy.None },
                            Faction = new Faction
                            {
                                name = (string)Station["controlling_faction"],
                                Allegiance = Superpower.FromName((string)Station["allegiance"]) ?? Superpower.None,
                                Government = Government.FromName((string)Station["government"]) ?? Government.None,
                            },
                            distancefromstar = (long?)Station["distance_to_star"],
                            updatedat = (long?)Station["updated_at"]
                        };
                        settlement.Faction.presences.Add(new FactionPresence()
                        {
                            systemName = system.systemname,
                            FactionState = FactionState.FromName((string)Station["state"]) ?? FactionState.None
                        });

                        system.stations.Add(settlement);
                    }
                }
            }
        }
    }
}