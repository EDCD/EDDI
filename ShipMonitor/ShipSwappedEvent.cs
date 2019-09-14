using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipSwappedEvent : Event
    {
        public const string NAME = "Ship swapped";
        public const string DESCRIPTION = "Triggered when you swap a ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardSwap\",\"ShipType\":\"Adder\",\"ShipID\":1,\"StoreOldShip\":\"Anaconda\",\"StoreShipID\":2, \"MarketID\": 128666762}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipSwappedEvent()
        {
            VARIABLES.Add("shipid", "The ID of the ship that was swapped");
            VARIABLES.Add("ship", "The ship that was swapped");
            VARIABLES.Add("soldshipid", "The ID of the ship that was sold as part of the swap");
            VARIABLES.Add("soldship", "The ship that was sold as part of the swap");
            VARIABLES.Add("storedshipid", "The ID of the ship that was stored as part of the swap");
            VARIABLES.Add("storedship", "The ship that was stored as part of the swap");
        }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("ship")]
        public string ship => shipDefinition?.model;

        [JsonProperty("soldshipid")]
        public int? soldshipid { get; private set; }

        [JsonProperty("soldship")]
        public string soldship => soldShipDefinition?.model;

        [JsonProperty("storedshipid")]
        public int? storedshipid { get; private set; }

        [JsonProperty("storedship")]
        public string storedship => storedShipDefinition?.model;

        // Not intended to be user facing
        public Ship shipDefinition { get; private set; }
        public Ship storedShipDefinition { get; private set; }
        public Ship soldShipDefinition { get; private set; }

        public long marketId { get; private set; }

        public ShipSwappedEvent(DateTime timestamp, string ship, int shipId, string soldship, int? soldshipid, string storedship, int? storedshipid, long marketId) : base(timestamp, NAME)
        {
            this.shipDefinition = ShipDefinitions.FromEDModel(ship);
            this.storedShipDefinition = ShipDefinitions.FromEDModel(storedship);
            this.soldShipDefinition = ShipDefinitions.FromEDModel(soldship);
            this.shipid = shipId;
            this.soldshipid = soldshipid;
            this.storedshipid = storedshipid;
            this.marketId = marketId;
        }
    }
}
