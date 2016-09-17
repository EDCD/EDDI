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
        public const string DESCRIPTION = "Triggered when you pick up cargo in your ship or SRV";
        public static CargoCollectedEvent SAMPLE = new CargoCollectedEvent(DateTime.Now, "Agricultural medicines", true);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoCollectedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CollectCargo\",\"Type\":\"agriculturalmedicines\",\"Stolen\":true}";

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
