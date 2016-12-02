using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class PowerCommodityFastTrackedEvent : Event
    {
        public const string NAME = "Power commodity fast tracked";
        public const string DESCRIPTION = "Triggered when a commander fast tracks a commodity of a power";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-16T09:28:19Z"", ""event"":""PowerplayFastTrack"", ""Power"":""Zachary Hudson"", ""Cost"":250000 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PowerCommodityFastTrackedEvent()
        {
            VARIABLES.Add("power", "The name of the power for which the commander is fast tracking the commodity");
            VARIABLES.Add("amount", "The amount spent in fast tracking");
        }

        public string power { get; private set; }

        public int amount { get; private set; }

        public PowerCommodityFastTrackedEvent(DateTime timestamp, string power, int amount) : base(timestamp, NAME)
        {
            this.power = power;
            this.amount = amount;
        }
    }
}
