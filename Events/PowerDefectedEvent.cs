using EddiDataDefinitions;
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

        public string frompower => (fromPower ?? Power.None)?.localizedName;

        public string topower => (toPower ?? Power.None)?.localizedName;

        // Not intended to be user facing

        public Power fromPower { get; private set; }
        public Power toPower { get; private set; }

        public PowerDefectedEvent(DateTime timestamp, Power fromPower, Power toPower) : base(timestamp, NAME)
        {
            this.fromPower = fromPower;
            this.toPower = toPower;
        }
    }
}
