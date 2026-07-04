using EddiDataDefinitions;
using EddiEddnResponder.Sender;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEddnResponder.Schemas
{
    [UsedImplicitly]
    public class OutfittingSchema : ISchema, ICapiSchema
    {
        public List<string> edTypes => [ "Outfitting" ];

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

                // Suppress repetitious messages less than 2 minutes apart.
                if ( lastSentMarketID == marketID && timestamp < ( lastSentDateTime + TimeSpan.FromMinutes( 2 ) ) )
                {
                    return false;
                }
                lastSentMarketID = marketID;
                lastSentDateTime = timestamp;

                if (data.TryGetValue("Items", out var modulesList))
                {
                    // Only send the message if we have modules
                    if (modulesList is List<object> modules && modules.Count > 0 )
                    {
                        var handledData = new Dictionary<string, object>() as IDictionary<string, object>;
                        handledData["timestamp"] = data["timestamp"];
                        handledData["systemName"] = data["StarSystem"];
                        handledData["stationName"] = data["StationName"]?.ToString()?.TrimEnd( '+', ' ' ); // Remove any +++ at the end of the station name
                        handledData["marketId"] = data["MarketID"];
                        handledData["modules"] = modules
                            .Select( m => m as Dictionary<string, object> )
                            .Where( m => ApplyModuleNameFilter( m[ "Name" ]?.ToString() ) )
                            .Where( m => !Module.IsPowerPlay( m[ "Name" ]?.ToString() ) )
                            .Select(AugmentBuyMercCoinsPrice)
                            .ToList();

                        // Apply data augments
                        handledData = eddnState.GameVersion.AugmentVersion(handledData);

                        eddnSender.SendToEDDN("https://eddn.edcd.io/schemas/outfitting/3", handledData, eddnState);
                        data = handledData;
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Error($"{GetType().Name} failed to handle journal data.", e);
            }
            return false;
        }

        public IDictionary<string, object> Handle ( JObject profileJson, JObject marketJson, JObject shipyardJson,
            JObject fleetCarrierJson, EDDNState eddnState, EDDNSender eddnSender )
        {
            try
            {
                // Modules are included in shipyardJson
                if (shipyardJson?["modules"] is null || eddnState?.GameVersion is null) { return null; }

                var systemName = profileJson?["lastSystem"]?["name"]?.ToString();
                var stationName = shipyardJson["name"]?.ToString().TrimEnd( '+', ' ' ); // Remove any +++ at the end of the station name
                var marketID = shipyardJson["id"].ToObject<long>();
                var timestamp = shipyardJson["timestamp"].ToObject<DateTime?>();

                // Sanity check - we must have a valid timestamp
                if ( timestamp == null ) { return null; }

                // Suppress repetitious messages less than 2 minutes apart.
                if ( lastSentMarketID == marketID && timestamp < ( lastSentDateTime + TimeSpan.FromMinutes( 2 ) ) )
                {
                    return null;
                }

                // Build our modules list
                var handledModules = new List<JToken>();
                foreach ( var jToken in shipyardJson[ "modules" ].Children().ToList() )
                {
                    // The modules collection can contain properties keyed by module id. Handle JProperty values as the module object.
                    var moduleToken = jToken.Type == JTokenType.Property ? ((JProperty)jToken).Value : jToken;
                    var module = moduleToken as JObject ?? JObject.FromObject(moduleToken);
                    var edName = module[ "name" ]?.ToString();
                    if ( !ApplyModuleNameFilter( edName ) || !ApplyModuleSkuFilter( module[ "sku" ]?.ToString() ) ) { continue; }
                    var handledModule = new JObject
                    {
                        ["id"] = module[ "id" ]?.ToObject<long>() ?? 0,
                        ["Name"] = edName,
                        ["BuyPrice"] = module[ "cost" ]?.ToObject<long>() ?? 0,
                        ["BuyMercCoinsPrice"] = module[ "BuyMercCoinsPrice" ]?.ToObject<long>() ?? 0,
                    };
                    handledModules.Add( handledModule );
                }

                // Continue if our modules list is not empty
                if ( handledModules.Count > 0 )
                {
                    var data = new Dictionary<string, object>() as IDictionary<string, object>;
                    data.Add("timestamp", timestamp);
                    data.Add("systemName", systemName);
                    data.Add("stationName", stationName);
                    data.Add("marketId", marketID);
                    data.Add("modules", handledModules);

                    // Apply data augments
                    data = eddnState.GameVersion.AugmentVersion(data);

                    eddnSender.SendToEDDN("https://eddn.edcd.io/schemas/outfitting/3", data, eddnState, "CAPI-Live-shipyard" );
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

        private static bool ApplyModuleNameFilter(string edName)
        {
            // Filter items that aren't weapons/utilities (Hpt_*), standard/internal modules (Int_*) or armour (*_Armour_*)
            // and the "Int_PlanetApproachSuite" module (for historical reasons)
            return (
                       edName.StartsWith("Int_", StringComparison.InvariantCultureIgnoreCase) ||
                       edName.StartsWith("Hpt_", StringComparison.InvariantCultureIgnoreCase) ||
                       edName.Contains("_Armour_") || edName.Contains("_armour_")
                   ) &&
                   edName != "Int_PlanetApproachSuite";
        }

        private static bool ApplyModuleSkuFilter ( string sku )
        {
            // Filter items that have a non-null "sku" property, unless it's "ELITE_HORIZONS_V_PLANETARY_LANDINGS" (i.e. PowerPlay and tech broker items).
            return string.IsNullOrEmpty( sku ) || sku.Equals( "ELITE_HORIZONS_V_PLANETARY_LANDINGS", StringComparison.InvariantCultureIgnoreCase );
        }

        private static Dictionary<string, object> AugmentBuyMercCoinsPrice ( Dictionary<string, object> moduleData )
        {
            if ( !moduleData.ContainsKey( "BuyMercCoinsPrice" ) )
            {
                moduleData.Add( "BuyMercCoinsPrice", 0 );
            }
            return moduleData;
        }
    }
}