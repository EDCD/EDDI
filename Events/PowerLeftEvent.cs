using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class PowerLeftEvent : Event
    {
        public const string NAME = "Power left";
        public const string DESCRIPTION = "Triggered when you leave a power";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-16T09:28:19Z"", ""event"":""PowerplayLeave"", ""Power"":""Zachary Hudson"" }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PowerLeftEvent()
        {
            VARIABLES.Add("power", "The name of the power that the commander has left");
        }

        public string power { get; private set; }

        public PowerLeftEvent(DateTime timestamp, string power) : base(timestamp, NAME)
        {
            this.power = power;
        }
    }
}
