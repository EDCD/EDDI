using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class TechnologyBrokerEvent : Event
    {
        public const string NAME = "Technology broker";
        public const string DESCRIPTION = "Triggered when using the Technology Broker to unlock new purchasable technology";
        public const string SAMPLE = "{ \"timestamp\":\"2018-03-02T11:28:44Z\", \"event\":\"TechnologyBroker\", \"BrokerType\":\"Human\", \"MarketID\":128151032, \"ItemsUnlocked\":[{ \"Name\":\"Hpt_PlasmaShockCannon_Fixed_Medium\", \"Name_Localised\":\"Shock Cannon\" }], \"Commodities\":[{ \"Name\":\"iondistributor\", \"Name_Localised\":\"Ion Distributor\", \"Count\":6 }], \"Materials\":[ { \"Name\":\"vanadium\", \"Count\":30, \"Category\":\"Raw\" }, { \"Name\":\"tungsten\", \"Count\":30, \"Category\":\"Raw\" }, { \"Name\":\"rhenium\", \"Count\":36, \"Category\":\"Raw\" }, { \"Name\":\"technetium\", \"Count\":30, \"Category\":\"Raw\"}]}";

        [PublicAPI("The technology broker's type (e.g. \"Human\")")]
        public string brokertype { get; private set; }

        [PublicAPI("The items unlocked in the transaction (as objects)")]
        public List<Module> items { get; private set; }

        [PublicAPI("The commodities and quantities used in the crafting (as objects)")]
        public List<CommodityAmount> commodities { get; private set; }

        [PublicAPI("The materials and quantities used in the crafting (as objects)")]
        public List<MaterialAmount> materials { get; private set; }

        // Not intended to be user facing

        public long marketid { get; private set; }

        public TechnologyBrokerEvent(DateTime timestamp, string brokerType, long marketId, List<Module> items, List<CommodityAmount> commodities, List<MaterialAmount> materials) : base(timestamp, NAME)
        {
            this.brokertype = brokerType;
            this.marketid = marketId;
            this.items = items;
            this.commodities = commodities;
            this.materials = materials;
        }
    }
}
