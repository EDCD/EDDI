using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MaterialTradedEvent : Event
    {
        public const string NAME = "Material traded";
        public const string DESCRIPTION = "Triggered when you trade materials at a materials trader";
        public const string SAMPLE = "{ \"timestamp\":\"2018-02-21T15:23:49Z\", \"event\":\"MaterialTrade\", \"MarketID\":3221397760, \"TraderType\":\"encoded\", \"Paid\":{ \"Material\":\"scandatabanks\", \"Material_Localised\":\"Classified Scan Databanks\", \"Category\":\"$MICRORESOURCE_CATEGORY_Encoded;\", \"Category_Localised\":\"Encoded\", \"Quantity\":6, \"Category\":\"$MICRORESOURCE_CATEGORY_Encoded;\", \"Category_Localised\":\"Encoded\" }, \"Received\":{ \"Material\":\"encodedscandata\", \"Material_Localised\":\"Divergent Scan Data\", \"Quantity\":1 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MaterialTradedEvent()
        {
            VARIABLES.Add("traderType", "The type of material trader for the trade");
            VARIABLES.Add("paid", "The name of the material lost in the trade");
            VARIABLES.Add("paidqty", "The amount of the material lost in the trade");
            VARIABLES.Add("received", "The name of the material lost in the trade");
            VARIABLES.Add("receivedqty", "The amount of the material lost in the trade");
        }

        [JsonProperty("tradertype")]
        public string tradertype { get; private set; }

        [JsonProperty("paid")]
        public string paid { get; private set; }

        [JsonProperty("paidqty")]
        public int paidqty { get; private set; }

        [JsonProperty("received")]
        public string received { get; private set; }

        [JsonProperty("receivedqty")]
        public int receivedqty { get; private set; }

        // Admin
        public long marketid { get; private set; }

        [JsonProperty("paid_edname")]
        public string paid_edname { get; private set; }

        [JsonProperty("received_edname")]
        public string received_edname { get; private set; }

        public MaterialTradedEvent(DateTime timestamp, long marketId, string traderType, Material materialPaid, int materialPaidQty, Material materialReceived, int materialReceivedQty) : base(timestamp, NAME)
        {
            this.marketid = marketId;
            this.tradertype = traderType;
            this.paid = materialPaid?.name;
            this.paidqty = materialPaidQty;
            this.paid_edname = materialPaid?.EDName;
            this.received = materialReceived?.name;
            this.receivedqty = materialReceivedQty;
            this.received_edname = materialReceived?.EDName;
        }
    }
}
