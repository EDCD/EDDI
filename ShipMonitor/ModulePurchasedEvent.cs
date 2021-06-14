using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ModulePurchasedEvent : Event
    {
        public const string NAME = "Module purchased";
        public const string DESCRIPTION = "Triggered when you purchase a module in outfitting";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleBuy\", \"MarketID\": 128666762, \"Slot\":\"MediumHardpoint2\", \"SellItem\":\"hpt_pulselaser_fixed_medium\", \"SellPrice\":0, \"BuyItem\":\"hpt_multicannon_gimbal_medium\", \"BuyPrice\":50018, \"Ship\":\"cobramkiii\", \"ShipID\":1  }";

        [PublicAPI("The ship for which the module was purchased")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship for which the module was purchased")]
        public int? shipid { get; private set; }

        [PublicAPI("The outfitting slot")]
        public string slot { get; private set; }

        [PublicAPI("The module (object) purchased")]
        public Module buymodule { get; private set; }

        [PublicAPI("The price of the module being purchased")]
        public long buyprice { get; private set; }

        [PublicAPI("The module (object) being sold (if replacing an existing module)")]
        public Module sellmodule { get; private set; }

        [PublicAPI("The price of the sold module (if replacing an existing module)")]
        public long? sellprice { get; private set; }

        [PublicAPI("The module (object) being stored (if existing module stored)")]
        public Module storedmodule { get; private set; }

        // Not intended to be user facing

        public long marketId { get; }

        public Ship shipDefinition { get; }

        public ModulePurchasedEvent(DateTime timestamp, string ship, int? shipid, string slot, Module buymodule, long buyprice, Module sellmodule, long? sellprice, Module storedmodule, long marketId) : base(timestamp, NAME)
        {
            this.shipDefinition = ShipDefinitions.FromEDModel(ship);
            this.shipid = shipid;
            this.slot = slot;
            this.buymodule = buymodule;
            this.buyprice = buyprice;
            this.sellmodule = sellmodule;
            this.sellprice = sellprice;
            this.storedmodule = storedmodule;
            this.marketId = marketId;
        }
    }
}
