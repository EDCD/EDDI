using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleStoredEvent (
        DateTime timestamp,
        string ship,
        int shipid,
        string slot,
        Module module,
        long? cost,
        string engineermodifications,
        Module replacementmodule,
        long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module stored";
        public const string DESCRIPTION = "Triggered when you store a module";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleStore\", \"MarketID\": 128666762, \"Slot\":\"MediumHardpoint2\", \"Ship\":\"cobramkiii\", \"ShipID\":1, \"Hot\":true, \"StoredItem\":\"hpt_pulselaser_fixed_medium\", \"EngineerModifications\":\"\", \"Cost\":500  }";

        [PublicAPI("The ship from which the module was stored")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship from which the module was stored")]
        public int shipid { get; private set; } = shipid;

        [PublicAPI("The outfitting slot")]
        public string slot { get; private set; } = slot;

        [PublicAPI("The module (object) being stored")]
        public Module module { get; private set; } = module;

        [PublicAPI("The cost of storage (if any)")]
        public long? cost { get; private set; } = cost;

        [PublicAPI("The name of the modification blueprint")]
        public string engineermodifications { get; private set; } = engineermodifications;

        [PublicAPI("The module (object) replacement (if a core module)")]
        public Module replacementmodule { get; private set; } = replacementmodule;

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;

        public Ship shipDefinition { get; private set; } = ShipDefinitions.FromEDModel(ship);
    }
}
