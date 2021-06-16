﻿using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiCargoMonitor
{
    public class CommodityEjectedEvent : Event
    {
        public const string NAME = "Commodity ejected";
        public const string DESCRIPTION = "Triggered when you eject a commodity from your ship or SRV";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"EjectCargo\",\"Type\":\"agriculturalmedicines\",\"Count\":2,\"Abandoned\":true}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommodityEjectedEvent()
        {
            VARIABLES.Add("commodity", "The name of the commodity ejected");
            VARIABLES.Add("amount", "The amount of cargo ejected");
            VARIABLES.Add("abandoned", "If the cargo has been abandoned");
            VARIABLES.Add("missionid", "ID of the mission-related commodity, if applicable");
        }

        [PublicAPI]
        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        [PublicAPI]
        public int amount { get; }

        [PublicAPI]
        public bool abandoned { get; }

        [PublicAPI]
        public long? missionid { get; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        public CommodityEjectedEvent(DateTime timestamp, CommodityDefinition commodity, int amount, long? missionid, bool abandoned) : base(timestamp, NAME)
        {
            this.commodityDefinition = commodity;
            this.amount = amount;
            this.missionid = missionid;
            this.abandoned = abandoned;
        }
    }
}
