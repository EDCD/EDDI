using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ModuleSwappedEvent : Event
    {
        public const string NAME = "Module swapped";
        public const string DESCRIPTION = "Triggered when modules are swapped between slots on the ship";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleSwap\", \"MarketID\": 128666762, \"FromSlot\":\"MediumHardpoint1\", \"ToSlot\":\"MediumHardpoint2\", \"FromItem\":\"hpt_pulselaser_fixed_medium\", \"ToItem\":\"hpt_multicannon_gimbal_medium\", \"Ship\":\"cobramkiii\", \"ShipID\":1 }";

        [PublicAPI("The ship for which the module was swapped")]
        public string ship { get; private set; }

        [PublicAPI("The ID of the ship for which the module was swapped")]
        public int? shipid { get; private set; }

        [PublicAPI("The slot from which the swap was initiated")]
        public string fromslot { get; private set; }

        [PublicAPI("The module (object) from which the swap was initiated")]
        public Module frommodule { get; private set; }

        [PublicAPI("The slot to which the swap was finalised")]
        public string toslot { get; private set; }

        [PublicAPI("The module (object) to which the swap was finalised")]
        public Module tomodule { get; private set; }

        // Not intended to be user facing

        public long marketId { get; private set; }

        public ModuleSwappedEvent(DateTime timestamp, string ship, int? shipid, string fromslot, Module frommodule, string toslot, Module tomodule, long marketId) : base(timestamp, NAME)
        {
            this.ship = ShipDefinitions.FromEDModel(ship).model;
            this.shipid = shipid;
            this.fromslot = fromslot;
            this.frommodule = frommodule;
            this.toslot = toslot;
            this.tomodule = tomodule;
            this.marketId = marketId;
        }
    }
}
