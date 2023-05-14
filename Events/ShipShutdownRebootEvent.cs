using System;
using Utilities;

namespace EddiEvents
{
    [ PublicAPI ]
    public class ShipShutdownRebootEvent : Event
    {
        public const string NAME = "Ship shutdown reboot";
        public const string DESCRIPTION = "Triggered when your ship's system reboots after a forced shutdown";
        public static ShipShutdownRebootEvent SAMPLE = new ShipShutdownRebootEvent( DateTime.UtcNow );

        public ShipShutdownRebootEvent ( DateTime timestamp ) : base( timestamp, NAME )
        { }
    }
}
