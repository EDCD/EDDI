using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipRebootedEvent : Event
    {
        public const string NAME = "Ship rebooted";
        public const string DESCRIPTION = "Triggered when you run reboot/repair on your ship";
        public static string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"RebootRepair\",\"Modules\":[\"MainEngines\",\"TinyHardpoint1\"]}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipRebootedEvent()
        {
            VARIABLES.Add("modules", "The modules that have been repaired");
        }

        [JsonProperty("modules")]
        public List<string> modules { get; private set; }

        public ShipRebootedEvent(DateTime timestamp, List<string> modules) : base(timestamp, NAME)
        {
            this.modules = modules;
        }
    }
}
