using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class ShipSwappedEvent : Event
    {
        public const string NAME = "Ship swapped";
        public const string DESCRIPTION = "Triggered when you swap a ship";
        public static ShipSwappedEvent SAMPLE = new ShipSwappedEvent(DateTime.Now, ShipDefinitions.ShipFromEDModel("Adder"), null, ShipDefinitions.ShipFromEDModel("Anaconda"));
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipSwappedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardSwap\",\"ShipType\":\"Adder\",\"ShipID\":1,\"StoreOldShip\":\"Anaconda\",\"StoreShipID\":2}";

            VARIABLES.Add("ship", "The ship that was swapped");
            VARIABLES.Add("soldship", "The ship that was sold as part of the swap");
            VARIABLES.Add("storedship", "The ship that was stored as part of the swap");
        }

        [JsonProperty("ship")]
        public Ship ship { get; private set; }

        [JsonProperty("soldship")]
        public Ship soldship { get; private set; }

        [JsonProperty("storedship")]
        public Ship storedship { get; private set; }

        public ShipSwappedEvent(DateTime timestamp, Ship ship, Ship soldShip, Ship storedShip) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.soldship = soldShip;
            this.storedship = storedShip;
        }
    }
}
