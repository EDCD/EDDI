using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipFsdEvent : Event
    {
        public const string NAME = "Ship fsd";
        public const string DESCRIPTION = "Triggered when there is a change to the status of your ship's fsd";
        public static ShipFsdEvent SAMPLE = new ShipFsdEvent(DateTime.UtcNow, "charging", true);

        [PublicAPI("The status of your ship's fsd ('cooldown', 'cooldown complete', 'charging', 'charging cancelled', 'charging complete', 'masslock', or 'masslock cleared')")]
        public string fsd_status { get; private set; }

        [PublicAPI("True if the FSD is currently charging for a jump to hyperspace.")]
        public bool hyperdrive_charging { get; private set; }

        public ShipFsdEvent(DateTime timestamp, string fsdStatus, bool hyperdriveCharging = false) : base(timestamp, NAME)
        {
            this.fsd_status = fsdStatus;
            this.hyperdrive_charging = hyperdriveCharging;
        }
    }
}
