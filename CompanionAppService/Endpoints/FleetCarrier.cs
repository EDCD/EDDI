using EddiDataDefinitions;
using EddiSpeechService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Utilities;

namespace EddiCompanionAppService
{
    public partial class CompanionAppService
    {
        private const string FLEETCARRIER_URL = "/fleetcarrier";

        // We cache data to avoid spamming the service
        private FrontierApiFleetCarrier cachedFleetCarrier;
        private DateTime cachedFleetCarrierExpires;

        public FrontierApiFleetCarrier FleetCarrier(bool forceRefresh = false)
        {
            if ((!forceRefresh) && cachedFleetCarrierExpires > DateTime.UtcNow)
            {
                // return the cached version
                Logging.Debug("Returning cached fleet carrier data");
                return cachedFleetCarrier;
            }

            try
            {
                string data = obtainData(ServerURL() + FLEETCARRIER_URL, out DateTime timestamp);

                if (data == null || !data.StartsWith("{"))
                {
                    // Happens if there is a problem with the API.  Logging in again might clear this...
                    relogin();
                    if (CurrentState != State.Authorized)
                    {
                        // No luck; give up
                        SpeechService.Instance.Say(null, Properties.CapiResources.frontier_api_lost, 0);
                        Logout();
                    }
                    else
                    {
                        // Looks like login worked; try again
                        data = obtainData(ServerURL() + FLEETCARRIER_URL, out timestamp);

                        if (data == null || !data.StartsWith("{"))
                        {
                            // No luck with a relogin; give up
                            SpeechService.Instance.Say(null, Properties.CapiResources.frontier_api_lost, 0);
                            Logout();
                            throw new EliteDangerousCompanionAppException("Failed to obtain data from Frontier server (" + CurrentState + ")");
                        }
                    }
                }

                try
                {
                    cachedFleetCarrier = FleetCarrierFromJson(data, timestamp);
                }
                catch (JsonException ex)
                {
                    Logging.Error("Failed to parse companion api fleet carrier data", ex);
                    cachedFleetCarrier = null;
                }
            }
            catch (EliteDangerousCompanionAppException ex)
            {
                // not Logging.Error as Rollbar is getting spammed when the server is down
                Logging.Info(ex.Message);
            }

            if (cachedFleetCarrier != null)
            {
                cachedFleetCarrierExpires = DateTime.UtcNow.AddSeconds(30);
                Logging.Debug("Fleet carrier is " + JsonConvert.SerializeObject(cachedFleetCarrier));
            }

            return cachedFleetCarrier;
        }

        /// <summary>Create a fleetcarrier given the results from a /fleetcarrier call</summary>
        public static FrontierApiFleetCarrier FleetCarrierFromJson(string data, DateTime timestamp)
        {
            if (!string.IsNullOrEmpty(data))
            {
                return FleetCarrierFromJson(JObject.Parse(data), timestamp);
            }
            return null;
        }

        /// <summary>Create a fleetcarrier given the results from a /fleetcarrier call</summary>
        public static FrontierApiFleetCarrier FleetCarrierFromJson(JObject json, DateTime timestamp)
        {
            FrontierApiFleetCarrier fleetCarrier = new FrontierApiFleetCarrier
            {
                json = json,
                timestamp = timestamp
            };

            // Name must be converted from a hexadecimal to a string
            string ConvertHexString(string hexString)
            {
                string ascii = string.Empty;
                for (int i = 0; i < hexString.Length; i += 2)
                {
                    var hs = hexString.Substring(i, 2);
                    var decval = Convert.ToUInt32(hs, 16);
                    var character = Convert.ToChar(decval);
                    ascii += character;
                }
                return ascii;
            }
            fleetCarrier.name = ConvertHexString(json["name"]["vanityName"]?.ToString());

            fleetCarrier.callsign = json["name"]?["callsign"]?.ToString();
            fleetCarrier.currentStarSystem = json["currentStarSystem"]?.ToString();
            fleetCarrier.fuel = int.Parse(json["fuel"]?.ToString());
            fleetCarrier.state = json["state"]?.ToString();
            fleetCarrier.dockingAccess = json["dockingAccess"]?.ToString();
            fleetCarrier.notoriousAccess = json["notoriousAccess"]?.ToObject<bool>() ?? false;

            // Capacity
            var shipPacks = json["capacity"]?["shipPacks"]?.ToObject<int>() ?? 0;
            var modulePacks = json["capacity"]?["modulePacks"]?.ToObject<int>() ?? 0;
            var cargoForSale = json["capacity"]?["cargoForSale"]?.ToObject<int>() ?? 0;
            var cargoNotForSale = json["capacity"]?["cargoNotForSale"]?.ToObject<int>() ?? 0;
            var reservedSpace = json["capacity"]?["cargoSpaceReserved"]?.ToObject<int>() ?? 0;
            var crew = json["capacity"]?["crew"]?.ToObject<int>() ?? 0;
            fleetCarrier.usedCapacity =
                shipPacks +
                modulePacks +
                cargoForSale +
                cargoNotForSale +
                reservedSpace +
                crew;
            fleetCarrier.freeCapacity = json["capacity"]?["freeSpace"]?.ToObject<int>() ?? 0;

            // Itinerary
            fleetCarrier.nextStarSystem = json["itinerary"]?["currentJump"]?.ToString();

            // Finances
            fleetCarrier.bankBalance = json["finance"]?["bankBalance"]?.ToObject<ulong>() ?? 0;
            fleetCarrier.bankReservedBalance = json["finance"]?["bankReservedBalance"]?.ToObject<ulong>() ?? 0;

            // Inventories
            fleetCarrier.Cargo = JArray.FromObject(json["cargo"]) ?? new JArray();
            fleetCarrier.CarrierLockerAssets = JArray.FromObject(json["carrierLocker"]?["assets"]) ?? new JArray();
            fleetCarrier.CarrierLockerGoods = JArray.FromObject(json["carrierLocker"]?["goods"]) ?? new JArray();
            fleetCarrier.CarrierLockerData = JArray.FromObject(json["carrierLocker"]?["data"]) ?? new JArray();

            // Market Buy/Sell Orders
            fleetCarrier.commodityPurchaseOrders = JArray.FromObject(json["orders"]?["commodities"]?["purchases"]) ?? new JArray();
            fleetCarrier.commoditySalesOrders = JArray.FromObject(json["orders"]?["commodities"]?["sales"]) ?? new JArray();
            fleetCarrier.microresourcePurchaseOrders = JArray.FromObject(json["orders"]?["onfootmicroresources"]?["purchases"]) ?? new JArray();
            fleetCarrier.microresourceSalesOrders = JArray.FromObject(json["orders"]?["onfootmicroresources"]?["sales"]) ?? new JArray();

            // Station properties
            fleetCarrier.Market = MarketFromJson(timestamp, json["market"]?.ToObject<JObject>());

            // Misc - Tritium stored in cargo
            foreach (var cargo in fleetCarrier.Cargo)
            {
                if (cargo["commodity"]?.ToString() is "Tritium")
                {
                    fleetCarrier.fuelInCargo += cargo["qty"].ToObject<int>();
                }
            }

            return fleetCarrier;
        }
    }
}