using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleSwappedEvent (
        DateTime timestamp,
        string ship,
        int? shipid,
        string fromslot,
        Module frommodule,
        string toslot,
        Module tomodule,
        long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module swapped";
        public const string DESCRIPTION = "Triggered when modules are swapped between slots on the ship";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleSwap\", \"MarketID\": 128666762, \"FromSlot\":\"MediumHardpoint1\", \"ToSlot\":\"MediumHardpoint2\", \"FromItem\":\"hpt_pulselaser_fixed_medium\", \"ToItem\":\"hpt_multicannon_gimbal_medium\", \"Ship\":\"cobramkiii\", \"ShipID\":1 }";

        [PublicAPI("The ship for which the module was swapped")]
        public string ship { get; private set; } = ShipDefinitions.FromEDModel(ship).model;

        [PublicAPI("The ID of the ship for which the module was swapped")]
        public int? shipid { get; private set; } = shipid;

        [PublicAPI("The slot from which the swap was initiated")]
        public string fromslot { get; private set; } = fromslot;

        [PublicAPI("The module (object) from which the swap was initiated")]
        public Module frommodule { get; set; } = frommodule;

        [PublicAPI("The slot to which the swap was finalised")]
        public string toslot { get; private set; } = toslot;

        [PublicAPI("The module (object) to which the swap was finalised")]
        public Module tomodule { get; set; } = tomodule;

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;
    }
}
