using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModulesStoredEvent (
        DateTime timestamp,
        string ship,
        int shipid,
        List<string> slots,
        List<Module> modules,
        long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Modules stored";
        public const string DESCRIPTION = "Triggered when you store multiple modules";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-18T16:42:53Z\", \"event\":\"MassModuleStore\", \"MarketID\": 128666762, \"Ship\":\"federation_corvette\", \"ShipID\":10, \"Items\":[ { \"Slot\":\"Slot02_Size7\", \"Name\":\"$int_shieldcellbank_size7_class5_name;\", \"Hot\":false }, { \"Slot\":\"Slot03_Size7\", \"Name\":\"$int_shieldcellbank_size7_class5_name;\", \"Hot\":false }, { \"Slot\":\"Slot05_Size6\", \"Name\":\"$int_cargorack_size6_class1_name;\", \"Hot\":true }, { \"Slot\":\"Slot07_Size5\", \"Name\":\"$int_fuelscoop_size5_class5_name;\", \"Hot\":false }] }";

        [PublicAPI("The ship from which the modules were stored")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship from which the module were stored")]
        public int shipid { get; private set; } = shipid;

        [PublicAPI("The outfitting slots")]
        public List<string> slots { get; private set; } = slots;

        [PublicAPI("The stored modules (as objects)")]
        public List<Module> modules { get; private set; } = modules;

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;

        public Ship shipDefinition { get; private set; } = ShipDefinitions.FromEDModel(ship);
    }
}
