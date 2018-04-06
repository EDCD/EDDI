using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class TechnologyBrokerEvent : Event
    {
        public const string NAME = "Technology broker";
        public const string DESCRIPTION = "Triggered when using the Technology Broker to unlock new purchasable technology";
        public const string SAMPLE = "{ \"timestamp\":\"2018-03-02T11:28:44Z\", \"event\":\"TechnologyBroker\", \"BrokerType\":\"Human\", \"MarketID\":128151032, \"ItemsUnlocked\":[{ \"Name\":\"Hpt_PlasmaShockCannon_Fixed_Medium\", \"Name_Localised\":\"Shock Cannon\" }], \"Commodities\":[{ \"Name\":\"iondistributor\", \"Name_Localised\":\"Ion Distributor\", \"Count\":6 }], \"Materials\":[ { \"Name\":\"vanadium\", \"Count\":30, \"Category\":\"Raw\" }, { \"Name\":\"tungsten\", \"Count\":30, \"Category\":\"Raw\" }, { \"Name\":\"rhenium\", \"Count\":36, \"Category\":\"Raw\" }, { \"Name\":\"technetium\", \"Count\":30, \"Category\":\"Raw\"}]}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static TechnologyBrokerEvent()
        {
            VARIABLES.Add("brokertype", "The technology broker's type (e.g. \"Human\")");
            VARIABLES.Add("items", "The items unlocked in the transaction (this is a list of Module objects)");
            VARIABLES.Add("materials", "The materials and quantities used in the crafting (MaterialAmount object with keys \"material\" and \"amount\", consisting of a material object and an amount)");
            VARIABLES.Add("commodities", "The commodities and quantities used in the crafting (CommodityAmount object with keys \"commodity\" and \"amount\", consisting of a commodity object and an amount)");
        }

        [JsonProperty("brokertype")]
        public string brokertype { get; private set; }

        [JsonProperty("items")]
        public List<Module> items { get; private set; }

        public List<CommodityAmount> commodities { get; private set; }

        public List<MaterialAmount> materials { get; private set; }

        // Admin
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
