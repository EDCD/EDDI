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
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardSwap\",\"ShipType\":\"Adder\",\"ShipID\":1,\"StoreOldShip\":\"Anaconda\",\"StoreShipID\":2}";
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
        public string ship { get; private set; }

        [JsonProperty("soldshipid")]
        public int? soldshipid { get; private set; }

        [JsonProperty("soldship")]
        public string soldship { get; private set; }

        [JsonProperty("storedshipid")]
        public int? storedshipid { get; private set; }

        [JsonProperty("storedship")]
        public string storedship { get; private set; }

        public ShipSwappedEvent(DateTime timestamp, string ship, int shipId, string soldship, int? soldshipid, string storedship, int? storedshipid) : base(timestamp, NAME)
        {
            this.ship = ShipDefinitions.FromEDModel(ship).model;
            this.shipid = shipId;
            this.soldship = soldship;
            this.soldshipid = soldshipid;
            this.storedship = storedship;
            this.storedshipid = storedshipid;
        }
    }
}
