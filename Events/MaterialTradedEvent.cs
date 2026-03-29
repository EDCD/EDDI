using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MaterialTradedEvent (
        DateTime timestamp,
        long marketId,
        string traderType,
        Material materialPaid,
        int materialPaidQty,
        Material materialReceived,
        int materialReceivedQty )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Material traded";
        public const string DESCRIPTION = "Triggered when you trade materials at a materials trader";
        public const string SAMPLE = "{ \"timestamp\": \"2018-04-02T05:04:45Z\", \"event\": \"MaterialTrade\", \"MarketID\": 3223343616, \"TraderType\": \"encoded\", \"Paid\": { \"Material\": \"shielddensityreports\", \"Material_Localised\": \"Untypical Shield Scans \", \"Category\": \"$MICRORESOURCE_CATEGORY_Encoded;\", \"Category_Localised\": \"Encoded\", \"Quantity\": 72 }, \"Received\": { \"Material\": \"shieldfrequencydata\", \"Material_Localised\": \"Peculiar Shield Frequency Data\", \"Category\": \"$MICRORESOURCE_CATEGORY_Encoded;\", \"Category_Localised\": \"Encoded\", \"Quantity\": 2 } }";

        [PublicAPI("The type of material trader for the trade")]
        public string tradertype { get; private set; } = traderType;

        [PublicAPI("The name of the material lost in the trade")]
        public string paid { get; private set; } = materialPaid?.localizedName;

        [PublicAPI("The amount of the material lost in the trade")]
        public int paid_quantity { get; private set; } = materialPaidQty;

        [PublicAPI("The name of the material gained in the trade")]
        public string received { get; private set; } = materialReceived?.localizedName;

        [PublicAPI("The amount of the material gained in the trade")]
        public int received_quantity { get; private set; } = materialReceivedQty;

        [PublicAPI( "The total amount of the material received in the trade in your inventory" )]
        public int received_total { get; set; }

        [PublicAPI( "The total amount of the material lost in the trade which remains in your inventory" )]
        public int lost_total { get; set; }

        // Not intended to be user facing

        public long marketid { get; private set; } = marketId;

        public string paid_edname { get; private set; } = materialPaid?.edname;

        public string received_edname { get; private set; } = materialReceived?.edname;
    }
}
