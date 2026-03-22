using EddiEddnResponder.Sender;
using EddiEddnResponder.Toolkit;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class CommoditySchema : ISchema, ICapiSchema
    {
        public List<string> edTypes => [ "Market", "MarketBuy", "MarketSell" ];

        // Track this so that we do not send duplicate data from the journal and from CAPI.
        private long? lastSentMarketID;
        private DateTime? lastSentDateTime;

        public bool Handle(string edType, ref IDictionary<string, object> data, EDDNState eddnState, EDDNSender eddnSender )
        {
            try
            {
                if (!edTypes.Contains(edType)) { return false; }
                if (eddnState?.GameVersion is null) { return false; }

                var marketID = JsonParsing.getLong(data, "MarketID");
                var timestamp = JsonParsing.getDateTime( "timestamp", data );

                // Reset message suppression if we've transacted with the market
                if ( marketID == lastSentMarketID &&
                     ( edType.Equals( "MarketBuy" ) ||
                       edType.Equals( "MarketSell" ) ) )
                {
                    lastSentDateTime = null;
                    return true;
                }

                // Suppress repetitious messages less than 2 minutes apart.
                if ( lastSentMarketID == marketID && timestamp < ( lastSentDateTime + TimeSpan.FromMinutes( 2 ) ) )
                {
                    return false;
                }
                lastSentMarketID = marketID;
                lastSentDateTime = timestamp;

                // Only send the message if we have commodities
                if (data.TryGetValue("Items", out var commoditiesList) &&
                    commoditiesList is List<object> list &&
                    list.Count > 0 )
                {
                    var handledData = new Dictionary<string, object>() as IDictionary<string, object>;
                    handledData["timestamp"] = data["timestamp"];
                    handledData["systemName"] = data["StarSystem"];
                    handledData["stationName"] = data["StationName"]?.ToString()?.TrimEnd( '+', ' ' ); // Remove any +++ at the end of the station name
                    handledData["stationType"] = data["StationType"]; // market.json specific
                    handledData["marketId"] = data["MarketID"];
                    handledData["commodities"] = JArray.FromObject(data["Items"])
                        .Where(c => ApplyJournalMarketFilter(c))
                        .Select(c => FormatCommodity(c, true))
                        .ToList();

                    if ( data.TryGetValue("CarrierDockingAccess", out var dockingAccess) )
                    {
                        handledData[ "carrierDockingAccess" ] = dockingAccess;
                    }

                    // Remove localized names
                    handledData = PersonalDataStripper.Strip(handledData, edType);

                    // Apply data augments
                    handledData = eddnState.GameVersion.AugmentVersion(handledData);

                    data = handledData;
                    eddnSender.SendToEDDN("https://eddn.edcd.io/schemas/commodity/3", handledData, eddnState);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logging.Error($"{GetType().Name} failed to handle journal data.", e);
            }
            return false;
        }

        private static bool ApplyJournalMarketFilter(JToken c)
        {
            // Don't serialize non-marketable commodities such as drones / limpets
            if (c?["Category"]?.ToString()
                    .Replace("$MARKET_category_", "")
                    .Replace(";", "")
                    .Replace("-", "")
                    .ToLowerInvariant() == "nonmarketable")
            {
                return false;
            }
            if (c?["Name"]?.ToString().ToLowerInvariant() == "drones")
            {
                return false;
            }
            return true;
        }

        public IDictionary<string, object> Handle ( JObject profileJson, JObject marketJson, JObject shipyardJson,
            JObject fleetCarrierJson, EDDNState eddnState, EDDNSender eddnSender )
        {
            try
            {
                if (marketJson?["commodities"] is null || eddnState?.GameVersion is null) { return null; }

                var systemName = profileJson?["lastSystem"]?["name"]?.ToString();
                var stationName = marketJson["name"]?.ToString().TrimEnd( '+', ' ' ); // Remove any +++ at the end of the station name
                var marketID = marketJson["id"].ToObject<long>();
                var timestamp = marketJson["timestamp"].ToObject<DateTime?>();

                // Sanity check - we must have a valid timestamp
                if ( timestamp == null ) { return null; }

                // Suppress repetitious messages less than 2 minutes apart.
                if ( lastSentMarketID == marketID && timestamp < ( lastSentDateTime + TimeSpan.FromMinutes( 2 ) ) )
                {
                    return null;
                }

                // Build our commodities lists
                var commodities = JArray.FromObject(marketJson["commodities"]?.ToObject<JArray>()?
                    .Where(c => ApplyFrontierApiMarketFilter(c))
                    .Select(c => FormatCommodity(c.ToObject<JObject>(), false)) ?? new List<JObject>());
                var prohibitedCommodities = marketJson["prohibited"]?.Children().Values();
                var economies = marketJson["economies"].Children().Values()
                    .Select(e => JObject.FromObject(e)).ToList();

                // Continue if our commodities list is not empty
                if (commodities.Any())
                {
                    var data = new Dictionary<string, object>() as IDictionary<string, object>;
                    data.Add("timestamp", timestamp);
                    data.Add("systemName", systemName);
                    data.Add("stationName", stationName);
                    data.Add("marketId", marketID);
                    data.Add("commodities", commodities);
                    data.Add("economies", economies);
                    data.Add("prohibited", prohibitedCommodities);

                    // Add fleet carrier data if applicable
                    if ( fleetCarrierJson != null )
                    {
                        if ( fleetCarrierJson["dockingAccess"]?.ToString() is string dockingAccess )
                        {
                            data.Add( "carrierDockingAccess", dockingAccess );
                        }
                    }

                    // Remove localized names
                    data = PersonalDataStripper.Strip(data);

                    // Apply data augments
                    data = eddnState.GameVersion.AugmentVersion(data);

                    eddnSender.SendToEDDN("https://eddn.edcd.io/schemas/commodity/3", data, eddnState, "CAPI-Live-market" );
                    lastSentMarketID = marketID;
                    lastSentDateTime = timestamp;
                    return data;
                }
            }
            catch (Exception e)
            {
                Logging.Error($"{GetType().Name} failed to handle Frontier API data.", e);
            }
            return null;
        }

        private static bool ApplyFrontierApiMarketFilter(JToken c)
        {
            try
            {
                // Don't serialize non-marketable commodities such as drones / limpets
                // Skip commodities with "categoryName": "NonMarketable"(i.e.Limpets - not purchasable in station market) or a non - empty"legality": string(not normally traded at this station market).

                if (!string.IsNullOrEmpty(c?["legality"]?.ToString()))
                {
                    return false;
                }
                if (c?["categoryname"]?.ToString().ToLowerInvariant() == "nonmarketable")
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                Logging.Error($"Failed to filter Frontier API commodity {JsonConvert.SerializeObject(c)}", e);
                return false;
            }
        }

        private static JObject FormatCommodity(JToken c, bool fromJournal)
        {
            var handledC = new JObject();
            if (fromJournal)
            {
                // Reformat commodity name to match the Frontier API
                handledC["name"] = c["Name"]?.ToString()
                    .Replace("$", "")
                    .Replace("_name", "")
                    .Replace(";", "");
                handledC["meanPrice"] = c["MeanPrice"];
                handledC["buyPrice"] = c["BuyPrice"];
                handledC["demand"] = c["Demand"];
                handledC["demandBracket"] = c["DemandBracket"];
                handledC["sellPrice"] = c["SellPrice"];
                handledC["stock"] = c["Stock"];
                handledC["stockBracket"] = c["StockBracket"];

                var statusFlags = new HashSet<string>();
                if (c["Producer"].ToObject<bool?>() == true)
                {
                    statusFlags.Add("Producer");
                }
                if (c["Consumer"].ToObject<bool?>() == true)
                {
                    statusFlags.Add("Consumer");
                }
                if (c["Rare"].ToObject<bool?>() == true)
                {
                    statusFlags.Add("Rare");
                }
                if (statusFlags.Count > 0 )
                {
                    handledC["statusFlags"] = JToken.FromObject(statusFlags);
                }
            }
            else
            {
                handledC["name"] = c["name"];
                handledC["meanPrice"] = c["meanPrice"];
                handledC["buyPrice"] = c["buyPrice"];
                handledC["demand"] = c["demand"];
                handledC["demandBracket"] = c["demandBracket"];
                handledC["sellPrice"] = c["sellPrice"];
                handledC["stock"] = c["stock"];
                handledC["stockBracket"] = c["stockBracket"];
                if (c["statusFlags"] is IEnumerable<object> statusFlags && statusFlags.Any())
                {
                    // Commodities may have more than one "powerplay" statusFlag.
                    handledC["statusFlags"] = JToken.FromObject( c["statusFlags"].Distinct() );
                }
            }
            return handledC;
        }
    }
}