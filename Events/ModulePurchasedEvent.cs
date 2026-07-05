using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModulePurchasedEvent (
        DateTime timestamp,
        string ship,
        int shipid,
        string slot,
        Module buymodule,
        long buyprice,
        long? buyMercPrice,
        Module sellmodule,
        long? sellprice,
        Module storedmodule,
        long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module purchased";
        public const string DESCRIPTION = "Triggered when you purchase a module in outfitting to install on your ship";
        public static readonly string[] SAMPLES =
        [
            "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleBuy\", \"MarketID\": 128666762, \"Slot\":\"MediumHardpoint2\", \"SellItem\":\"hpt_pulselaser_fixed_medium\", \"SellPrice\":0, \"BuyItem\":\"hpt_multicannon_gimbal_medium\", \"BuyPrice\":50018, \"Ship\":\"cobramkiii\", \"ShipID\":1  }",
            "{ \"timestamp\":\"2025-05-15T05:09:11Z\", \"event\":\"ModuleBuy\", \"Slot\":\"Slot10_Size1\", \"StoredItem\":\"$int_detailedsurfacescanner_tiny_name;\", \"StoredItem_Localised\":\"Surface Scanner\", \"BuyItem\":\"$int_dockingcomputer_advanced_name;\", \"BuyItem_Localised\":\"Docking Computer\", \"MarketID\":4243222787, \"BuyPrice\":13170, \"Ship\":\"cutter\", \"ShipID\":54 }",
            "{ \"timestamp\":\"2024-05-06T01:46:03Z\", \"event\":\"ModuleBuy\", \"Slot\":\"Slot01_Size6\", \"StoredItem\":\"$int_shieldgenerator_size6_class3_fast_name;\", \"StoredItem_Localised\":\"Bi-Weave Shield\", \"BuyItem\":\"$int_hullreinforcement_size5_class2_name;\", \"BuyItem_Localised\":\"Hull Reinforcement\", \"MarketID\":129019519, \"BuyPrice\":438750, \"Ship\":\"typex\", \"ShipID\":76 }"
        ];
        
        [PublicAPI("The ship for which the module was purchased")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship for which the module was purchased")]
        public int shipid { get; private set; } = shipid;

        [PublicAPI("The outfitting slot")]
        public string slot { get; private set; } = slot;

        [PublicAPI("The module (object) purchased")]
        public Module buymodule { get; private set; } = buymodule;

        [PublicAPI("The price of the module being purchased, in credits")]
        public long buyprice { get; private set; } = buyprice;

        [PublicAPI( "The price of the module being purchased, in merc coin, when applicable" )]
        public long? buymercprice { get; private set; } = buyMercPrice;

        [PublicAPI("The module (object) being sold (if replacing an existing module)")]
        public Module sellmodule { get; private set; } = sellmodule;

        [PublicAPI("The price of the sold module (if replacing an existing module)")]
        public long? sellprice { get; private set; } = sellprice;

        [PublicAPI("The module (object) being stored (if existing module stored)")]
        public Module storedmodule { get; private set; } = storedmodule;

        // Not intended to be user facing

        public long marketId { get; } = marketId;

        public Ship shipDefinition { get; } = ShipDefinitions.FromEDModel(ship);

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data,
            ref List<Event> events, bool fromLogLoad )
        {
            var marketId = JsonParsing.getLong( data, "MarketID" );
            var shipId = JsonParsing.getInt( data, "ShipID" );
            var ship = JsonParsing.getString( data, "Ship" );
            var slot = JsonParsing.getString( data, "Slot" );
            
            var buyModule = Module.FromEDName( JsonParsing.getString( data, "BuyItem" ) );
            buyModule.fallbackLocalizedName = JsonParsing.getString( data, "BuyItem_Localised" );
            var buyPrice = JsonParsing.getLong( data, "BuyPrice" );
            buyModule.price = buyPrice;
            var buyMercPrice = JsonParsing.getOptionalLong( data, "BuyMercCoinsPrice" );
            buyModule.mercPrice = buyMercPrice;

            // Set retrieved module defaults
            buyModule.enabled = true;
            buyModule.priority = 1;
            buyModule.health = 100;
            buyModule.modified = false;

            var sellModule = Module.FromEDName( JsonParsing.getString( data, "SellItem" ) );
            if ( sellModule != null )
            {
                sellModule.fallbackLocalizedName = JsonParsing.getString( data, "SellItem_Localised" );
            }
            var sellPrice = JsonParsing.getOptionalLong( data, "SellPrice" );
            
            var storedModule = Module.FromEDName( JsonParsing.getString( data, "StoredItem" ) );
            if ( storedModule != null )
            {
                storedModule.fallbackLocalizedName = JsonParsing.getString( data, "StoredItem_Localised" );
            }

            events.Add( new ModulePurchasedEvent( timestamp, ship, shipId, slot, buyModule, buyPrice, buyMercPrice, sellModule, sellPrice, storedModule, marketId ) { raw = line, fromLoad = fromLogLoad } );

            return true;
        }
    }
}
