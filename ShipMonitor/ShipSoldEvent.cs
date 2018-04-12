using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipSoldEvent : Event
    {
        public const string NAME = "Ship sold";
        public const string DESCRIPTION = "Triggered when you sell a ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardSell\",\"ShipType\":\"Adder\",\"SellShipID\":1,\"ShipPrice\":25000}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipSoldEvent()
        {
            VARIABLES.Add("ship", "The ship that was sold");
            VARIABLES.Add("shipid", "The ID of the ship that was sold");
            VARIABLES.Add("price", "The price for which the ship was sold");
            VARIABLES.Add("system", "The system where the ship was sold");
        }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("price")]
        public long price { get; private set; }
        
        [JsonProperty("system")]
        public string system { get; private set; }        

        public ShipSoldEvent(DateTime timestamp, string ship, int shipId, long price, string system) : base(timestamp, NAME)
        {
            this.ship = ShipDefinitions.FromEDModel(ship).model;
            this.shipid = shipId;
            this.price = price;
            this.system = system;
        }
    }
}
