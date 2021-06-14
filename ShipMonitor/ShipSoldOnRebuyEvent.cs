using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ShipSoldOnRebuyEvent : Event
    {
        public const string NAME = "Ship sold on rebuy";
        public const string DESCRIPTION = "Triggered when you sell a ship to raise funds on the rebuy screen";
        public const string SAMPLE = "{\"timestamp\":\"2017-07-20T08:56:39Z\", \"event\":\"SellShipOnRebuy\", \"ShipType\":\"Dolphin\", \"System\":\"Shinrarta Dezhra\", \"SellShipId\":4, \"ShipPrice\":4110183}";

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

        public string edModel { get; private set; }

        public ShipSoldOnRebuyEvent(DateTime timestamp, string ship, int shipId, long price, string system) : base(timestamp, NAME)
        {
            this.edModel = ship;
            this.shipid = shipId;
            this.price = price;
            this.system = system;
        }
    }
}
