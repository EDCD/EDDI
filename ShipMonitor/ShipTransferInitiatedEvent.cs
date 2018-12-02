using EddiDataDefinitions;
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
        public const string SAMPLE = "{ \"timestamp\":\"2018-07-30T04:56:57Z\", \"event\":\"ShipyardTransfer\", \"ShipType\":\"Krait_MkII\", \"ShipType_Localised\":\"Krait MkII\", \"ShipID\":81, \"System\":\"Balante\", \"ShipMarketID\":3223259392, \"Distance\":8.017741, \"TransferPrice\":134457, \"TransferTime\":380, \"MarketID\":3223343616 }";
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

        // Admin
        public long fromMarketId { get; private set; }
        public long toMarketId { get; private set; }

        public ShipTransferInitiatedEvent(DateTime timestamp, string ship, int? shipid, string system, decimal distance, long? price, long? time, long fromMarketId, long toMarketId) : base(timestamp, NAME)
        {
            this.ship = ShipDefinitions.FromEDModel(ship).model;
            this.shipid = shipid;
            this.system = system;
            this.distance = distance;
            this.price = price;
            this.time = time;
            this.fromMarketId = fromMarketId;
            this.toMarketId = toMarketId;
        }
    }
}
