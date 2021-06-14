using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class PowerDefectedEvent : Event
    {
        public const string NAME = "Power defected";
        public const string DESCRIPTION = "Triggered when you defect from one power to another";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-16T09:28:19Z"", ""event"":""PowerplayDefect"", ""FromPower"":""Zachary Hudson"", ""ToPower"":""Li Yong-Rui"" }";

        [PublicAPI("The name of the power that the commander has defected from")]
        public string frompower => (fromPower ?? Power.None)?.localizedName;

        [PublicAPI("The name of the power that the commander has defected to")]
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
