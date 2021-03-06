using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiShipMonitor
{
    public class ShipRebootedEvent : Event
    {
        public const string NAME = "Ship rebooted";
        public const string DESCRIPTION = "Triggered when you run reboot/repair on your ship";
        public static ShipRebootedEvent SAMPLE = new ShipRebootedEvent(DateTime.UtcNow, new List<Module>() { Module.FromEDName("modularcargobaydoor"), Module.FromEDName("int_powerplant_size2_class5"), Module.FromEDName("int_engine_size7_class2"), Module.FromEDName("hpt_plasmapointdefence_turret_tiny") });
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipRebootedEvent()
        {
            VARIABLES.Add("modules", "The localized module names that have been repaired");
            VARIABLES.Add("modules_invariant", "The invariant module names that have been repaired");
        }

        public List<string> modules => Modules?.Select(m => m.localizedName).ToList();

        public List<string> modules_invariant => Modules?.Select(m => m.invariantName).ToList();
        
        // Not intended to be user facing
        public List<Module> Modules { get; private set; }

        public ShipRebootedEvent(DateTime timestamp, List<Module> Modules) : base(timestamp, NAME)
        {
            this.Modules = Modules;
        }
    }
}
