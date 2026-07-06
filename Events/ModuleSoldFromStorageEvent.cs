using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleSoldFromStorageEvent (
        DateTime timestamp,
        string ship,
        int? shipid,
        int storageslot,
        long serverid,
        Module module,
        long price )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module sold from storage";
        public const string DESCRIPTION = "Triggered when selling a module in storage";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-20T02:28:37Z\", \"event\":\"ModuleSellRemote\", \"StorageSlot\":10, \"SellItem\":\"$int_fueltank_size4_class3_name;\", \"SellItem_Localised\":\"Fuel Tank\", \"ServerId\":128064349, \"SellPrice\":24116, \"Ship\":\"diamondbackxl\", \"ShipID\":38 }";

        [PublicAPI("The ship from which the module was sold")]
        public string ship { get; private set; } = ShipDefinitions.FromEDModel(ship).model;

        [PublicAPI("The ID of the ship from which the module was sold")]
        public int? shipid { get; private set; } = shipid;

        [PublicAPI("The module (object) being sold")]
        public Module module { get; private set; } = module;

        [PublicAPI("The price of the module being sold")]
        public long price { get; private set; } = price;

        // Not intended to be user facing

        public int storageslot { get; private set; } = storageslot;

        public long serverid { get; private set; } = serverid;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            data.TryGetValue( "ShipID", out var val );
            var shipId = (int)(long)val;
            var ship = JsonParsing.getString(data, "Ship");

            var module = Module.FromEDName(JsonParsing.getString(data, "SellItem"));
            data.TryGetValue( "SellPrice", out val );
            var price = (long)val;

            // Probably not useful. We'll get these but we won't tell the end user about them
            data.TryGetValue( "StorageSlot", out val );
            var storageSlot = (int)(long)val;
            data.TryGetValue( "ServerId", out val );
            var serverId = (long)val;

            events.Add( new ModuleSoldFromStorageEvent( timestamp, ship, shipId, storageSlot, serverId, module, price ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
