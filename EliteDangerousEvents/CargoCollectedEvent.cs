using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class CargoCollectedEvent : Event
    {
        public const string NAME = "Cargo collected";
        public static CargoCollectedEvent SAMPLE = new CargoCollectedEvent(DateTime.Now, "Biowaste", true);

        static CargoCollectedEvent()
        {
            VARIABLES.Add("cargo", "The type of cargo collected");
            VARIABLES.Add("stolen", "If the cargo is stolen");
        }

        [JsonProperty("cargo")]
        public string cargo { get; private set; }

        [JsonProperty("stolen")]
        public bool stolen { get; private set; }

        public CargoCollectedEvent(DateTime timestamp, string cargo, bool stolen) : base(timestamp, NAME)
        {
            this.cargo = cargo;
            this.stolen = stolen;
        }
    }
}
