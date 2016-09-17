using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class CargoEjectedEvent : Event
    {
        public const string NAME = "Cargo ejected";
        public const string DESCRIPTION = "Triggered when you eject cargo from your ship or SRV";
        public static CargoEjectedEvent SAMPLE = new CargoEjectedEvent(DateTime.Now, "Agricultural medicines", 5, true);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoEjectedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"EjectCargo\",\"Type\":\"agriculturalmedicines\",\"Count\":2,\"Abandoned\":true}";

            VARIABLES.Add("cargo", "The type of cargo ejected");
            VARIABLES.Add("amount", "The amount of cargo ejected");
            VARIABLES.Add("abandoned", "If the cargo has been abandoned");
        }

        [JsonProperty("cargo")]
        public string cargo { get; private set; }

        [JsonProperty("amount")]
        public int amount { get; private set; }

        [JsonProperty("abandoned")]
        public bool abandoned { get; private set; }

        public CargoEjectedEvent(DateTime timestamp, string cargo, int amount, bool abandoned) : base(timestamp, NAME)
        {
            this.cargo = cargo;
            this.amount = amount;
            this.abandoned = abandoned;
        }
    }
}
