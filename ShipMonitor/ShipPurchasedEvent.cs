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
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-20T18:14:26Z\", \"event\":\"ShipyardBuy\", \"ShipType\":\"federation_corvette\", \"ShipPrice\":18796945, \"SellOldShip\":\"CobraMkIII\", \"SellShipID\":42, \"SellPrice\":950787, \"MarketID\": 128666762 }";
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
        public string ship => shipDefinition?.model;

        [JsonProperty("price")]
        public long price { get; private set; }

        [JsonProperty("soldship")]
        public string soldship => soldShipDefinition?.model;

        [JsonProperty("soldshipid")]
        public int? soldshipid { get; private set; }

        [JsonProperty("soldprice")]
        public long? soldprice { get; private set; }

        [JsonProperty("storedship")]
        public string storedship => storedShipDefinition?.model;

        [JsonProperty("storedshipid")]
        public int? storedshipid { get; private set; }

        // Not intended to be user facing
        public Ship shipDefinition { get; private set; }
        public Ship storedShipDefinition { get; private set; }
        public Ship soldShipDefinition { get; private set; }
        public long marketId { get; private set; }

        public ShipPurchasedEvent(DateTime timestamp, string ship, long price, string soldShip, int? soldShipId, long? soldPrice, string storedShip, int? storedShipId, long marketId) : base(timestamp, NAME)
        {
            this.shipDefinition = ShipDefinitions.FromEDModel(ship);
            this.storedShipDefinition = ShipDefinitions.FromEDModel(storedship);
            this.soldShipDefinition = ShipDefinitions.FromEDModel(soldship);
            this.price = price;
            this.soldshipid = soldShipId;
            this.soldprice = soldPrice;
            this.storedshipid = storedShipId;
            this.marketId = marketId;
        }
    }
}
