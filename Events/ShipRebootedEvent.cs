using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipRebootedEvent ( DateTime timestamp, List<string> compartments ) : Event( timestamp, NAME )
    {
        public const string NAME = "Ship rebooted";
        public const string DESCRIPTION = "Triggered when you run reboot/repair on your ship";
        public static readonly ShipRebootedEvent SAMPLE = new(DateTime.UtcNow, [ ] ) { Modules =
            [
                Module.FromEDName( "modularcargobaydoor" ), Module.FromEDName( "int_powerplant_size2_class5" ),
                Module.FromEDName( "int_engine_size7_class2" ),
                Module.FromEDName( "hpt_plasmapointdefence_turret_tiny" )
            ]
        };

        [PublicAPI("The localized module names that have been repaired")]
        public List<string> modules => Modules?.Select(m => m.localizedName).ToList();

        [PublicAPI("The invariant module names that have been repaired")]
        public List<string> modules_invariant => Modules?.Select(m => m.invariantName).ToList();

        // Not intended to be user facing

        public List<string> compartments { get; private set; } = compartments;

        public List<Module> Modules { get; set; } = [ ]; // Set via the Ship Monitor, referencing the current ship
    }
}
