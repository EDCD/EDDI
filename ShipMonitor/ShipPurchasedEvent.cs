using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipPurchasedEvent : Event
    {
        public const string NAME = "Ship purchased";
        public const string DESCRIPTION = "Triggered when you purchase a ship";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-20T18:14:26Z\", \"event\":\"ShipyardBuy\", \"ShipType\":\"federation_corvette\", \"ShipPrice\":18796945, \"SellOldShip\":\"CobraMkIII\", \"SellShipID\":42, \"SellPrice\":950787 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipPurchasedEvent()
        {
            VARIABLES.Add("ship", "The ship that was purchased");
            VARIABLES.Add("price", "The price of the ship that was purchased");
            VARIABLES.Add("soldship", "The ship that was sold as part of the purchase");
            VARIABLES.Add("soldshipid", "The ID of the ship that was sold as part of the purchase");
            VARIABLES.Add("soldprice", "The credits obtained by selling the ship");
            VARIABLES.Add("storedship", "The ship that was stored as part of the purchase");
            VARIABLES.Add("storedid", "The ID of the ship that was stored as part of the purchase");
        }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("price")]
        public long price { get; private set; }

        [JsonProperty("soldship")]
        public string  soldship { get; private set; }

        [JsonProperty("soldshipid")]
        public int? soldshipid { get; private set; }

        [JsonProperty("soldprice")]
        public long? soldprice { get; private set; }

        [JsonProperty("storedship")]
        public string storedship { get; private set; }

        [JsonProperty("storedshipid")]
        public int? storedshipid { get; private set; }

        public ShipPurchasedEvent(DateTime timestamp, string ship, long price, string soldShip, int? soldShipId, long? soldPrice, string storedShip, int? storedShipId) : base(timestamp, NAME)
        {
            this.ship = ShipDefinitions.FromEDModel(ship).model;
            this.price = price;
            this.soldship = soldShip;
            this.soldshipid = soldShipId;
            this.soldprice = soldPrice;
            this.storedship = storedShip;
            this.storedshipid = storedShipId;
        }
    }
}
