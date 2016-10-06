using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
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
            VARIABLES.Add("soldname", "The name of the ship that was sold as part of the purchase");
            VARIABLES.Add("soldprice", "The credits obtained by selling the ship");
            VARIABLES.Add("storedship", "The ship that was stored as part of the purchase");
            VARIABLES.Add("storedname", "The name of the ship that was stored as part of the purchase");
        }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("price")]
        public long price { get; private set; }

        [JsonProperty("soldship")]
        public string  soldship { get; private set; }

        [JsonProperty("soldname")]
        public string soldname { get; private set; }

        [JsonProperty("soldprice")]
        public long? soldprice { get; private set; }

        [JsonProperty("storedship")]
        public string storedship { get; private set; }

        [JsonProperty("storedname")]
        public string storedname { get; private set; }

        public ShipPurchasedEvent(DateTime timestamp, Ship ship, long price, Ship soldShip, long? soldPrice, Ship storedShip) : base(timestamp, NAME)
        {
            this.ship = (ship == null ? null : ship.model);
            this.price = price;
            this.soldship = (soldShip == null ? null : soldShip.model);
            this.soldname = (soldShip == null ? null : soldShip.name);
            this.soldprice = soldPrice;
            this.storedship = (storedShip == null ? null : storedShip.model);
            this.storedname = (storedShip == null ? null : storedShip.name);
        }
    }
}
