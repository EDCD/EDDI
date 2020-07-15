using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class MaterialTradedEvent : Event
    {
        public const string NAME = "Material traded";
        public const string DESCRIPTION = "Triggered when you trade materials at a materials trader";
        public const string SAMPLE = "{ \"timestamp\": \"2018-04-02T05:04:45Z\", \"event\": \"MaterialTrade\", \"MarketID\": 3223343616, \"TraderType\": \"encoded\", \"Paid\": { \"Material\": \"shielddensityreports\", \"Material_Localised\": \"Untypical Shield Scans \", \"Category\": \"$MICRORESOURCE_CATEGORY_Encoded;\", \"Category_Localised\": \"Encoded\", \"Quantity\": 72 }, \"Received\": { \"Material\": \"shieldfrequencydata\", \"Material_Localised\": \"Peculiar Shield Frequency Data\", \"Category\": \"$MICRORESOURCE_CATEGORY_Encoded;\", \"Category_Localised\": \"Encoded\", \"Quantity\": 2 } }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MaterialTradedEvent()
        {
            VARIABLES.Add("tradertype", "The type of material trader for the trade");
            VARIABLES.Add("paid", "The name of the material lost in the trade");
            VARIABLES.Add("paid_quantity", "The amount of the material lost in the trade");
            VARIABLES.Add("received", "The name of the material gained in the trade");
            VARIABLES.Add("received_quantity", "The amount of the material gained in the trade");
        }

        public string tradertype { get; private set; }

        public string paid { get; private set; }

        public int paid_quantity { get; private set; }

        public string received { get; private set; }

        public int received_quantity { get; private set; }

        // Not intended to be user facing

        [VoiceAttackIgnore]
        public long marketid { get; private set; }

        [VoiceAttackIgnore]
        public string paid_edname { get; private set; }

        [VoiceAttackIgnore]
        public string received_edname { get; private set; }

        public MaterialTradedEvent(DateTime timestamp, long marketId, string traderType, Material materialPaid, int materialPaidQty, Material materialReceived, int materialReceivedQty) : base(timestamp, NAME)
        {
            this.marketid = marketId;
            this.tradertype = traderType;
            this.paid = materialPaid?.localizedName;
            this.paid_quantity = materialPaidQty;
            this.paid_edname = materialPaid?.edname;
            this.received = materialReceived?.localizedName;
            this.received_quantity = materialReceivedQty;
            this.received_edname = materialReceived?.edname;
        }
    }
}
