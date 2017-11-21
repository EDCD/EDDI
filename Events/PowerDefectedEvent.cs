using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class PowerDefectedEvent : Event
    {
        public const string NAME = "Power defected";
        public const string DESCRIPTION = "Triggered when you defect from one power to another";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-16T09:28:19Z"", ""event"":""PowerplayDefect"", ""FromPower"":""Zachary Hudson"", ""ToPower"":""Li Yong-Rui"" }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PowerDefectedEvent()
        {
            VARIABLES.Add("frompower", "The name of the power that the commander has defected from");
            VARIABLES.Add("topower", "The name of the power that the commander has defected to");
        }

        public string frompower { get; private set; }

        public string topower { get; private set; }

        public PowerDefectedEvent(DateTime timestamp, string frompower, string topower) : base(timestamp, NAME)
        {
            this.frompower = frompower;
            this.topower = topower;
        }
    }
}
