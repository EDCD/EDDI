using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleSoldEvent (
        DateTime timestamp,
        string ship,
        int shipid,
        string slot,
        Module module,
        long price,
        long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module sold";
        public const string DESCRIPTION = "Triggered when selling a module to outfitting";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleSell\", \"MarketID\": 128666762, \"Slot\":\"Slot06_Size2\", \"SellItem\":\"int_cargorack_size1_class1\", \"SellPrice\":877, \"Ship\":\"asp\", \"ShipID\":1 }";

        [PublicAPI("The ship from which the module was sold")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship from which the module was sold")]
        public int shipid { get; private set; } = shipid;

        [PublicAPI("The outfitting slot")]
        public string slot { get; private set; } = slot;

        [PublicAPI("The module (object) being sold")]
        public Module module { get; private set; } = module;

        [PublicAPI("The price of the module being sold")]
        public long price { get; private set; } = price;

        // Not intended to be user facing

        public long marketId { get; } = marketId;

        public Ship shipDefinition { get; } = ShipDefinitions.FromEDModel(ship);
    }
}