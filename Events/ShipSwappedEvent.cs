using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipSwappedEvent (
        DateTime timestamp,
        string ship,
        int shipId,
        string soldship,
        int? soldshipid,
        string storedship,
        int? storedshipid,
        long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Ship swapped";
        public const string DESCRIPTION = "Triggered when you swap a ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardSwap\",\"ShipType\":\"Adder\",\"ShipID\":1,\"StoreOldShip\":\"Anaconda\",\"StoreShipID\":2, \"MarketID\": 128666762}";

        [PublicAPI("The ID of the ship that was swapped")]
        public int? shipid { get; private set; } = shipId;

        [PublicAPI("The ship that was swapped")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship that was sold as part of the swap")]
        public int? soldshipid { get; private set; } = soldshipid;

        [PublicAPI("The ship that was sold as part of the swap")]
        public string soldship => soldShipDefinition?.model;

        [PublicAPI("The ID of the ship that was stored as part of the swap")]
        public int? storedshipid { get; private set; } = storedshipid;

        [PublicAPI("The ship that was stored as part of the swap")]
        public string storedship => storedShipDefinition?.model;

        // Not intended to be user facing

        public Ship shipDefinition => ShipDefinitions.FromEDModel(edModel);

        public Ship storedShipDefinition => string.IsNullOrEmpty(storedEdModel) ? null : ShipDefinitions.FromEDModel(storedEdModel);

        public Ship soldShipDefinition => string.IsNullOrEmpty(soldEdModel) ? null : ShipDefinitions.FromEDModel(soldEdModel);

        public string edModel { get; private set; } = ship;

        public string storedEdModel { get; private set; } = storedship;

        public string soldEdModel { get; private set; } = soldship;

        public long marketId { get; private set; } = marketId;
    }
}
