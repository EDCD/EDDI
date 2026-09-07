using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipTransferInitiatedEvent (
        DateTime timestamp,
        Ship ship,
        string system,
        decimal distance,
        long? price,
        long? time,
        long fromMarketId,
        long toMarketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Ship transfer initiated";
        public const string DESCRIPTION = "Triggered when you initiate a ship transfer";
        public const string SAMPLE = "{ \"timestamp\":\"2018-07-30T04:56:57Z\", \"event\":\"ShipyardTransfer\", \"ShipType\":\"Krait_MkII\", \"ShipType_Localised\":\"Krait MkII\", \"ShipID\":81, \"System\":\"Balante\", \"ShipMarketID\":3223259392, \"Distance\":8.017741, \"TransferPrice\":134457, \"TransferTime\":380, \"MarketID\":3223343616 }";

        [PublicAPI("The ID of the ship that is being transferred")]
        public int? shipid => Ship.LocalId;

        [PublicAPI("The (invariant) ship model that is being transferred")]
        public string ship => Ship.model;

        [PublicAPI("The phonetic name of the ship that is being transferred")]
        public string phoneticname => Ship.phoneticname;

        [PublicAPI("The system from which the ship is being transferred")]
        public string system { get; private set; } = system;

        [PublicAPI("The distance that the transferred ship needs to travel, in light years")]
        public decimal distance { get; private set; } = distance;

        [PublicAPI("The price of transferring the ship")]
        public long? price { get; private set; } = price;

        [PublicAPI("The time in seconds to complete transferring the ship")]
        public long? time { get; private set; } = time;

        // Not intended to be user facing

        public long fromMarketId { get; private set; } = fromMarketId;

        public long toMarketId { get; private set; } = toMarketId;

        public Ship Ship { get; private set; } = ship;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var toMarketId = JsonParsing.getLong(data, "MarketID");
            var fromMarketId = JsonParsing.getLong(data, "ShipMarketID");

            data.TryGetValue( "ShipID", out var val );
            var shipId = (int)(long)val;

            var system = JsonParsing.getString(data, "System");
            var distance = JsonParsing.getDecimal(data, "Distance");
            var price = JsonParsing.getOptionalLong(data, "TransferPrice");
            var time = JsonParsing.getOptionalLong(data, "TransferTime");

            var shipEDModel = JsonParsing.getString(data, "ShipType");
            var ship = ShipDefinitions.FromEDModel(shipEDModel);
            ship.LocalId = shipId;

            events.Add( new ShipTransferInitiatedEvent( timestamp, ship, system, distance, price, time, fromMarketId, toMarketId ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }

        public void SetShip ( Ship resolvedShip ) => Ship = resolvedShip ?? Ship;
    }
}
