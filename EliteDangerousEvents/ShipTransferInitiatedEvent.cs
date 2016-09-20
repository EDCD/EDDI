using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class ShipTransferInitiatedEvent : Event
    {
        public const string NAME = "Ship transfer initiated";
        public const string DESCRIPTION = "Triggered when you initiate a ship transfer";
        public static ShipTransferInitiatedEvent SAMPLE = new ShipTransferInitiatedEvent(DateTime.Now, ShipDefinitions.FromEDModel("Adder"), "Eranin", 85.639145M, 580M);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipTransferInitiatedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardTransfer\",\"ShipType\":\"Adder\",\"ShipID\":1,\"System\":\"Eranin\",\"Distance\":85.639145,\"TransferPrice\":580}";

            VARIABLES.Add("ship", "The ship that is being transferred");
            VARIABLES.Add("system", "The system from which the ship is being transferred");
            VARIABLES.Add("distance", "The distance that the transferred ship needs to travel");
            VARIABLES.Add("cost", "The cost of transferring the ship");
        }

        [JsonProperty("ship")]
        public Ship ship { get; private set; }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("distance")]
        public decimal distance { get; private set; }

        [JsonProperty("cost")]
        public decimal cost { get; private set; }

        public ShipTransferInitiatedEvent(DateTime timestamp, Ship ship, string system, decimal distance, decimal cost) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.system = system;
            this.distance = distance;
            this.cost = cost;
        }
    }
}
