using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiCargoMonitor
{
    public class CargoUpdatedEvent : Event
    {
        public const string NAME = "Cargo updated";
        public const string DESCRIPTION = "Triggered when the cargo inventory is updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoUpdatedEvent()
        {
            VARIABLES.Add("cargocarried", "The current tonnage cargo carried");
        }

        [JsonProperty("cargocarried")]
        public int cargocarried { get; private set; }

        public CargoUpdatedEvent(DateTime timestamp, int cargocarried) : base(timestamp, NAME)
        {
            this.cargocarried = cargocarried;
        }
    }
}
