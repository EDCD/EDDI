﻿using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiCargoMonitor
{
    public class PowerCommodityObtainedEvent : Event
    {
        public const string NAME = "Power commodity obtained";
        public const string DESCRIPTION = "Triggered when a commander obtains a commodity from a power";
        public const string SAMPLE = "{ \"timestamp\":\"2016-12-02T16:10:26Z\", \"event\":\"PowerplayCollect\", \"Power\":\"Aisling Duval\", \"Type\":\"$aislingmediamaterials_name;\", \"Type_Localised\":\"Aisling Media Materials\", \"Count\":3 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PowerCommodityObtainedEvent()
        {
            VARIABLES.Add("power", "The name of the power for which the commander is obtaining the commodity");
            VARIABLES.Add("commodity", "The name of the commodity the commander is obtaining");
            VARIABLES.Add("amount", "The amount of the commodity the commander is obtaining");
        }

        public string power => (Power ?? Power.None).localizedName;

        public string commodity => commodityDefinition?.localizedName;

        public int amount { get; private set; }

        // Not intended to be user facing

        public Power Power { get; private set; }

        public CommodityDefinition commodityDefinition { get; private set; }

        public PowerCommodityObtainedEvent(DateTime timestamp, Power Power, CommodityDefinition commodity, int amount) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.amount = amount;
            this.commodityDefinition = commodity;
        }
    }
}
