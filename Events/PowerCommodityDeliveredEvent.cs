using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class PowerCommodityDeliveredEvent : Event
    {
        public const string NAME = "Power commodity delivered";
        public const string DESCRIPTION = "Triggered when a commander delivers a commodity to a power";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-02T16:10:26Z"", ""event"":""PowerplayDeliver"", ""Power"":""Aisling Duval"", ""Type"":""$aislingmediamaterials_name;"", ""Type_Localised"":""Aisling Media Materials"", ""Count"":3 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PowerCommodityDeliveredEvent()
        {
            VARIABLES.Add("power", "The name of the power for which the commander is delivering the commodity");
            VARIABLES.Add("commodity", "The commodity the commander is delivering");
            VARIABLES.Add("amount", "The amount of the commodity the commander is delivering");
        }

        public string power { get; private set; }

        public string commodity { get; private set; }

        public int amount { get; private set; }

        public PowerCommodityDeliveredEvent(DateTime timestamp, string power, string commodity, int amount) : base(timestamp, NAME)
        {
            this.power = power;
            this.commodity = commodity;
            this.amount = amount;
        }
    }
}
