using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipFsdEvent : Event
    {
        public const string NAME = "Ship fsd";
        public const string DESCRIPTION = "Triggered when there is a change to the status of your ship's fsd";
        public const string SAMPLE = null;

        [PublicAPI("The status of your ship's fsd ('cooldown', 'cooldown complete', 'charging', 'charging cancelled', 'charging complete', 'masslock', or 'masslock cleared')")]
        public string fsd_status { get; private set; }

        public ShipFsdEvent(DateTime timestamp, string fsdStatus) : base(timestamp, NAME)
        {
            this.fsd_status = fsdStatus;
        }
    }
}
