using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipSoldOnRebuyEvent : Event
    {
        public const string NAME = "Ship sold on rebuy";
        public const string DESCRIPTION = "Triggered when you sell a ship to raise funds on the rebuy screen";
        public const string SAMPLE = "{\"timestamp\":\"2017-07-20T08:56:39Z\", \"event\":\"SellShipOnRebuy\", \"ShipType\":\"Dolphin\", \"System\":\"Shinrarta Dezhra\", \"SellShipId\":4, \"ShipPrice\":4110183}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipSoldOnRebuyEvent()
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

        public ShipSoldOnRebuyEvent(DateTime timestamp, string ship, int shipId, long price, string system) : base(timestamp, NAME)
        {
            this.ship = ship;      
            this.shipid = shipId;
            this.price = price;
            this.system = system;
        }
    }
}
