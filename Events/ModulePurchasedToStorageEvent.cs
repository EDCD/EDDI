using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModulePurchasedToStorageEvent ( DateTime timestamp, Module buymodule, long buyprice, long? buyMercPrice, long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module purchased to storage";
        public const string DESCRIPTION = "Triggered when you purchase and store a module without installing it on your ship";
        public static readonly string[] SAMPLES =
        [
            "{ \"timestamp\":\"2024-09-21T09:17:12Z\", \"event\":\"ModuleBuyAndStore\", \"BuyItem\":\"$int_multidronecontrol_operations_size3_class4_name;\", \"BuyItem_Localised\":\"Operations Multi-Limpet Controller\", \"MarketID\":3228926720, \"BuyPrice\":78000, \"Ship\":\"typex\", \"ShipID\":76 }",
            "{ \"timestamp\":\"2025-01-13T06:48:41Z\", \"event\":\"ModuleBuyAndStore\", \"BuyItem\":\"$int_guardianfsdbooster_size4_name;\", \"BuyItem_Localised\":\"Guardian FSD Booster\", \"MarketID\":3230492672, \"BuyPrice\":3163887, \"Ship\":\"cobramkv\", \"ShipID\":128 }",
            "{ \"timestamp\":\"2021-08-02T06:18:05Z\", \"event\":\"ModuleBuyAndStore\", \"BuyItem\":\"$int_supercruiseassist_name;\", \"BuyItem_Localised\":\"Supercruise Assist\", \"MarketID\":3228552448, \"BuyPrice\":8893, \"Ship\":\"diamondbackxl\", \"ShipID\":38 }"
        ];

        [PublicAPI("The module (object) purchased")]
        public Module buymodule { get; private set; } = buymodule;

        [PublicAPI("The price of the module being purchased, in credits")]
        public long buyprice { get; private set; } = buyprice;

        [PublicAPI( "The price of the module being purchased, in merc coin, when applicable" )]
        public long? buymercprice { get; private set; } = buyMercPrice;

        // Not intended to be user facing

        public long marketId { get; } = marketId;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var marketId = JsonParsing.getLong(data, "MarketID");

            var buyModule = Module.FromEDName(JsonParsing.getString(data, "BuyItem"));
            buyModule.fallbackLocalizedName = JsonParsing.getString( data, "BuyItem_Localised" );
            var buyPrice = JsonParsing.getLong( data, "BuyPrice" );
            buyModule.price = buyPrice;
            var buyMercPrice = JsonParsing.getLong( data, "BuyMercCoinsPrice" );
            buyModule.mercPrice = buyMercPrice;

            // Set retrieved module defaults
            buyModule.enabled = true;
            buyModule.priority = 1;
            buyModule.health = 100;
            buyModule.modified = false;

            events.Add( new ModulePurchasedToStorageEvent( timestamp, buyModule, buyPrice, buyMercPrice, marketId ) { raw = line, fromLoad = fromLogLoad } );

            return true;
        }
    }
}
