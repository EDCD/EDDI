using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ShipSoldEvent : Event
    {
        public const string NAME = "Ship sold";
        public const string DESCRIPTION = "Triggered when you sell a ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardSell\",\"ShipType\":\"Adder\",\"SellShipID\":1,\"ShipPrice\":25000, \"MarketID\": 128666762}";

        [PublicAPI("The ship that was sold")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship that was sold")]
        public int? shipid { get; private set; }

        [PublicAPI("The price for which the ship was sold")]
        public long price { get; private set; }

        [PublicAPI("The system where the ship was sold")]
        public string system { get; private set; }

        // Not intended to be user facing

        public Ship shipDefinition => ShipDefinitions.FromEDModel(edModel);

        public long marketId { get; private set; }

        public string edModel { get; private set; }

        public ShipSoldEvent(DateTime timestamp, string ship, int shipId, long price, string system, long marketId) : base(timestamp, NAME)
        {
            this.edModel = ship;
            this.shipid = shipId;
            this.price = price;
            this.system = system;
            this.marketId = marketId;
        }
    }
}
