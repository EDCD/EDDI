using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipTransferInitiatedEvent : Event
    {
        public const string NAME = "Ship transfer initiated";
        public const string DESCRIPTION = "Triggered when you initiate a ship transfer";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardTransfer\",\"ShipType\":\"Adder\",\"ShipID\":1,\"System\":\"Eranin\",\"Distance\":85.639145,\"TransferPrice\":580,\"TransferTime\":120}";
        //public const string SAMPLE = "{ \"timestamp\":\"2017-09-24T12:31:38Z\", \"event\":\"ShipyardTransfer\", \"ShipType\":\"Asp\", \"ShipID\":2, \"System\":\"CD-34 9020\", \"Distance\":145.314835, \"TransferPrice\":127713 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipTransferInitiatedEvent()
        {
            VARIABLES.Add("shipid", "The ID of the ship that is being transferred");
            VARIABLES.Add("ship", "The ship that is being transferred");
            VARIABLES.Add("system", "The system from which the ship is being transferred");
            VARIABLES.Add("distance", "The distance that the transferred ship needs to travel, in light years");
            VARIABLES.Add("price", "The price of transferring the ship");
            VARIABLES.Add("time", "The time in seconds to complete transferring the ship");
        }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("distance")]
        public decimal distance { get; private set; }

        [JsonProperty("price")]
        public long? price { get; private set; }

        [JsonProperty("time")]
        public long? time { get; private set; }

        public ShipTransferInitiatedEvent(DateTime timestamp, string ship, int? shipid, string system, decimal distance, long? price, long? time) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.shipid = shipid;
            this.system = system;
            this.distance = distance;
            this.price = price;
            this.time = time;
        }
    }
}
