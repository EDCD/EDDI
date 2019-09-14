using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class PowerJoinedEvent : Event
    {
        public const string NAME = "Power joined";
        public const string DESCRIPTION = "Triggered when you join a power";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-16T09:28:19Z"", ""event"":""PowerplayJoin"", ""Power"":""Zachary Hudson"" }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PowerJoinedEvent()
        {
            VARIABLES.Add("power", "The name of the power that the commander has joined");
        }

        public string power => (Power ?? Power.None).localizedName;

        // Not intended to be user facing

        public Power Power { get; private set; }

        public PowerJoinedEvent(DateTime timestamp, Power Power) : base(timestamp, NAME)
        {
            this.Power = Power;
        }
    }
}
