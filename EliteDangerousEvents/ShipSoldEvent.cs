using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class ShipSoldEvent : Event
    {
        public const string NAME = "Ship sold";
        public const string DESCRIPTION = "Triggered when you sell a ship";
        public static ShipSoldEvent SAMPLE = new ShipSoldEvent(DateTime.Now, ShipDefinitions.ShipFromEDModel("Adder"), 25000M);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipSoldEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardSell\",\"ShipType\":\"Adder\",\"SellShipID\":1,\"ShipPrice\":25000}";

            VARIABLES.Add("ship", "The ship that was sold");
            VARIABLES.Add("price", "The price for which the ship was sold");
        }

        [JsonProperty("ship")]
        public Ship ship { get; private set; }

        [JsonProperty("price")]
        public decimal price { get; private set; }

        public ShipSoldEvent(DateTime timestamp, Ship ship, decimal price) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.price = price;
        }
    }
}
