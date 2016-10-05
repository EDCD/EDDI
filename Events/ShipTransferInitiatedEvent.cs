using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class ShipTransferInitiatedEvent : Event
    {
        public const string NAME = "Ship transfer initiated";
        public const string DESCRIPTION = "Triggered when you initiate a ship transfer";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardTransfer\",\"ShipType\":\"Adder\",\"ShipID\":1,\"System\":\"Eranin\",\"Distance\":85.639145,\"TransferPrice\":580}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipTransferInitiatedEvent()
        {
            VARIABLES.Add("ship", "The ship that is being transferred");
            VARIABLES.Add("system", "The system from which the ship is being transferred");
            VARIABLES.Add("distance", "The distance that the transferred ship needs to travel, in light years");
            VARIABLES.Add("price", "The price of transferring the ship");
        }

        [JsonProperty("ship")]
        public Ship ship { get; private set; }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("distance")]
        public decimal distance { get; private set; }

        [JsonProperty("price")]
        public decimal price { get; private set; }

        public ShipTransferInitiatedEvent(DateTime timestamp, Ship ship, string system, decimal distance, decimal price) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.system = system;
            this.distance = distance;
            this.price = price;
        }
    }
}
