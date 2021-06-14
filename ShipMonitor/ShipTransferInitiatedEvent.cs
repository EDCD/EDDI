using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ShipTransferInitiatedEvent : Event
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
        public string system { get; private set; }

        [PublicAPI("The distance that the transferred ship needs to travel, in light years")]
        public decimal distance { get; private set; }

        [PublicAPI("The price of transferring the ship")]
        public long? price { get; private set; }

        [PublicAPI("The time in seconds to complete transferring the ship")]
        public long? time { get; private set; }

        // Not intended to be user facing

        public long fromMarketId { get; private set; }

        public long toMarketId { get; private set; }
        
        public Ship Ship { get; private set; }

        public ShipTransferInitiatedEvent(DateTime timestamp, Ship Ship, string system, decimal distance, long? price, long? time, long fromMarketId, long toMarketId) : base(timestamp, NAME)
        {
            this.Ship = Ship;
            this.system = system;
            this.distance = distance;
            this.price = price;
            this.time = time;
            this.fromMarketId = fromMarketId;
            this.toMarketId = toMarketId;
        }
    }
}
