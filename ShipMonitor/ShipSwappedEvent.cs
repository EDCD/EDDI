using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ShipSwappedEvent : Event
    {
        public const string NAME = "Ship swapped";
        public const string DESCRIPTION = "Triggered when you swap a ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardSwap\",\"ShipType\":\"Adder\",\"ShipID\":1,\"StoreOldShip\":\"Anaconda\",\"StoreShipID\":2, \"MarketID\": 128666762}";

        [PublicAPI("The ID of the ship that was swapped")]
        public int? shipid { get; private set; }

        [PublicAPI("The ship that was swapped")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship that was sold as part of the swap")]
        public int? soldshipid { get; private set; }

        [PublicAPI("The ship that was sold as part of the swap")]
        public string soldship => soldShipDefinition?.model;

        [PublicAPI("The ID of the ship that was stored as part of the swap")]
        public int? storedshipid { get; private set; }

        [PublicAPI("The ship that was stored as part of the swap")]
        public string storedship => storedShipDefinition?.model;

        // Not intended to be user facing

        public Ship shipDefinition => ShipDefinitions.FromEDModel(edModel);

        public Ship storedShipDefinition => string.IsNullOrEmpty(storedEdModel) ? null : ShipDefinitions.FromEDModel(storedEdModel);

        public Ship soldShipDefinition => string.IsNullOrEmpty(soldEdModel) ? null : ShipDefinitions.FromEDModel(soldEdModel);

        public string edModel { get; private set; }

        public string storedEdModel { get; private set; }

        public string soldEdModel { get; private set; }
        
        public long marketId { get; private set; }

        public ShipSwappedEvent(DateTime timestamp, string ship, int shipId, string soldship, int? soldshipid, string storedship, int? storedshipid, long marketId) : base(timestamp, NAME)
        {
            this.edModel = ship;
            this.storedEdModel = storedship;
            this.soldEdModel = soldship;
            this.shipid = shipId;
            this.soldshipid = soldshipid;
            this.storedshipid = storedshipid;
            this.marketId = marketId;
        }
    }
}
