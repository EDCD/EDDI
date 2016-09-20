using EliteDangerousDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class ShipPurchasedEvent : Event
    {
        public const string NAME = "Ship purchased";
        public const string DESCRIPTION = "Triggered when you purchase a ship";
        public static ShipPurchasedEvent SAMPLE = new ShipPurchasedEvent(DateTime.Now, ShipDefinitions.FromEDModel("Adder"), null, null, ShipDefinitions.FromEDModel("Anaconda"));
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipPurchasedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardBuy\",\"ShipType\":\"Adder\",\"ShipID\":1,\"StoreOldShip\":\"Anaconda\",\"StoreShipID\":2}";

            VARIABLES.Add("ship", "The ship that was purchased");
            VARIABLES.Add("soldship", "The ship that was sold as part of the purchase");
            VARIABLES.Add("soldprice", "The credits obtained by selling the ship");
            VARIABLES.Add("storedship", "The ship that was stored as part of the purchase");
        }

        [JsonProperty("ship")]
        public Ship ship { get; private set; }

        [JsonProperty("soldship")]
        public Ship soldship { get; private set; }

        [JsonProperty("soldprice")]
        public decimal? soldprice { get; private set; }

        [JsonProperty("storedship")]
        public Ship storedship { get; private set; }

        public ShipPurchasedEvent(DateTime timestamp, Ship ship, Ship soldShip, decimal? soldPrice, Ship storedShip) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.soldship = soldShip;
            this.soldprice = soldPrice;
            this.storedship = storedShip;
        }
    }
}
