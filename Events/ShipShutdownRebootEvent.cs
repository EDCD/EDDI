using System;
using Utilities;

namespace EddiEvents
{
    [ PublicAPI ]
    public class ShipShutdownRebootEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Ship shutdown reboot";
        public const string DESCRIPTION = "Triggered when your ship's system reboots after a forced shutdown";
        public static readonly ShipShutdownRebootEvent SAMPLE = new( DateTime.UtcNow );
    }
}
