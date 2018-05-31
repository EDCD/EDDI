using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace EddiMissionMonitor
{
    public class CargoDepotEvent : Event
    {
        public const string NAME = "Cargo depot";
        public const string DESCRIPTION = "Triggered when collecting or delivering cargo for a wing mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-25T12:53:01Z\", \"event\":\"CargoDepot\", \"MissionID\":26493517, \"UpdateType\":\"Deliver\", \"CargoType\":\"SyntheticMeats\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoDepotEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("updatetype", "The update type. One of: 'Collect', 'Deliver', 'WingUpdate'");
            VARIABLES.Add("cargotype", "The type of cargo (commodity)");
            VARIABLES.Add("count", "The amount of cargo collected or delivered for this event");
            VARIABLES.Add("collected", "The amount of cargo collected");
            VARIABLES.Add("delivered", "The amount of cargo delivered");
            VARIABLES.Add("totaltodeliver", "The total amount of cargo to deliver to complete the mission");
            VARIABLES.Add("progress", "The total amount of cargo to delivered");
        }

        [JsonProperty("missionid")]
        public long? missionid { get; private set; }

        [JsonProperty("updatetype")]
        public string updatetype { get; private set; }

        [JsonProperty("cargotype")]
        public string cargotype { get; private set; }

        [JsonProperty("count")]
        public int? count { get; private set; }

        [JsonProperty("collected")]
        public int collected { get; private set; }

        [JsonProperty("delivered")]
        public int delivered { get; private set; }

        [JsonProperty("totaltodeliver")]
        public int totaltodeliver { get; private set; }

        [JsonProperty("progress")]
        public decimal progress { get; private set; }

        public CargoDepotEvent(DateTime timestamp, long? missionid, string updatetype, CommodityDefinition cargotype, int? count, int collected, int delivered, int totaltodeliver, decimal progress) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.updatetype = updatetype;
            this.cargotype = cargotype?.localizedName;
            this.count = count;
            this.collected = collected;
            this.delivered = delivered;
            this.totaltodeliver = totaltodeliver;
            this.progress = progress;
        }
    }
}
