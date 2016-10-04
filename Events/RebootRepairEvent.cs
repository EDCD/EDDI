using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class RebootRepairEvent : Event
    {
        public const string NAME = "Reboot repair";
        public const string DESCRIPTION = "Triggered when you run reboot/repair on your ship";
        public static string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"RebootRepair\",\"Modules\":[\"MainEngines\",\"TinyHardpoint1\"]}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static RebootRepairEvent()
        {
            VARIABLES.Add("modules", "The modules that have been repaired");
        }

        [JsonProperty("modules")]
        public List<string> modules { get; private set; }

        public RebootRepairEvent(DateTime timestamp, List<string> modules) : base(timestamp, NAME)
        {
            this.modules = modules;
        }
    }
}
