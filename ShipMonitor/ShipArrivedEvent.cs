using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipArrivedEvent : Event
    {
        public const string NAME = "Ship arrived";
        public const string DESCRIPTION = "Triggered when you complete a ship transfer";
        public static ShipArrivedEvent SAMPLE = new ShipArrivedEvent(DateTime.UtcNow, "Adder", 1, "Eranin", 85.639145M, 580, 30, "Xuesen Orbital", 128035840, 3222994688);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipArrivedEvent()
        {
            VARIABLES.Add("shipid", "The ID of the ship that was transferred");
            VARIABLES.Add("ship", "The ship that was transferred");
            VARIABLES.Add("station", "The station at which the ship shall arrive");
            VARIABLES.Add("system", "The system at which the ship shall arrive");
            VARIABLES.Add("distance", "The distance that the transferred ship travelled, in light years");
            VARIABLES.Add("price", "The price of transferring the ship");
            VARIABLES.Add("time", "The time elapsed during the transfer (in seconds)");
        }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        public string station { get; private set; }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("distance")]
        public decimal distance { get; private set; }

        [JsonProperty("price")]
        public long? price { get; private set; }

        [JsonProperty("time")]
        public long? time { get; private set; }

        // Admin
        public long fromMarketId { get; private set; }
        public long toMarketId { get; private set; }

        public ShipArrivedEvent(DateTime timestamp, string ship, int? shipid, string system, decimal distance, long? price, long? time, string station, long fromMarketId, long toMarketId) : base(timestamp, NAME)
        {
            this.ship = ShipDefinitions.FromEDModel(ship).model;
            this.shipid = shipid;
            this.station = station;
            this.system = system;
            this.distance = distance;
            this.price = price;
            this.time = time;
            this.fromMarketId = fromMarketId;
            this.toMarketId = toMarketId;
        }
    }
}
