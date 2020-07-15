using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class ModulePurchasedEvent : Event
    {
        public const string NAME = "Module purchased";
        public const string DESCRIPTION = "Triggered when you purchase a module in outfitting";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleBuy\", \"MarketID\": 128666762, \"Slot\":\"MediumHardpoint2\", \"SellItem\":\"hpt_pulselaser_fixed_medium\", \"SellPrice\":0, \"BuyItem\":\"hpt_multicannon_gimbal_medium\", \"BuyPrice\":50018, \"Ship\":\"cobramkiii\", \"ShipID\":1  }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModulePurchasedEvent()
        {
            VARIABLES.Add("ship", "The ship for which the module was purchased");
            VARIABLES.Add("shipid", "The ID of the ship for which the module was purchased");
            VARIABLES.Add("slot", "The outfitting slot");
            VARIABLES.Add("buymodule", "The module (object) purchased");
            VARIABLES.Add("buyprice", "The price of the module being purchased");
            VARIABLES.Add("sellmodule", "The module (object) being sold (if replacing an existing module)");
            VARIABLES.Add("sellprice", "The price of the sold module (if replacing an existing module)");
            VARIABLES.Add("storedmodule", "The module (object) being stored (if existing module stored)");
        }

        public string ship => shipDefinition?.model;

        public int? shipid { get; private set; }

        public string slot { get; private set; }

        public Module buymodule { get; private set; }

        public long buyprice { get; private set; }

        public Module sellmodule { get; private set; }

        public long? sellprice { get; private set; }

        public Module storedmodule { get; private set; }

        // Not intended to be user facing

        [VoiceAttackIgnore]
        public long marketId { get; private set; }

        [VoiceAttackIgnore]
        public Ship shipDefinition { get; private set; }

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
