using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ShipArrivedEvent : Event
    {
        public const string NAME = "Ship arrived";
        public const string DESCRIPTION = "Triggered when you complete a ship transfer";
        public static ShipArrivedEvent SAMPLE = new ShipArrivedEvent(DateTime.Parse("2016-06-10T14:32:03Z").ToUniversalTime(), ShipDefinitions.FromEDModel("CobraMkIII"), "Eranin", 85.639145M, 580, 30, "Azeban City", 128168184, 128001536);

        [PublicAPI("The ID of the ship that was transferred")]
        public int? shipid => Ship.LocalId;

        [PublicAPI("The (invariant) ship model that was transferred")]
        public string ship => Ship.model;

        [PublicAPI("The phonetic name of the ship that was transferred")]
        public string phoneticname => Ship.phoneticname;

        [PublicAPI("The station at which the ship shall arrive")]
        public string station { get; private set; }

        [PublicAPI("The system at which the ship shall arrive")]
        public string system { get; private set; }

        [PublicAPI("The distance that the transferred ship travelled, in light years")]
        public decimal distance { get; private set; }

        [PublicAPI("The price of transferring the ship")]
        public long? price { get; private set; }

        [PublicAPI("The time elapsed during the transfer (in seconds)")]
        public long? time { get; private set; }

        // Not intended to be user facing

        public long fromMarketId { get; private set; }

        public long toMarketId { get; private set; }

        public Ship Ship { get; private set; }

        public ShipArrivedEvent(DateTime timestamp, Ship Ship, string system, decimal distance, long? price, long? time, string station, long fromMarketId, long toMarketId) : base(timestamp, NAME)
        {
            this.Ship = Ship;
            this.station = station;
            this.system = system;
            this.distance = distance;
            this.price = price;
            this.time = time;
            this.fromMarketId = fromMarketId;
            this.toMarketId = toMarketId;
        }
    }
}
