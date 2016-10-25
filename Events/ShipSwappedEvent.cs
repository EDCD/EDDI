using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
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

        [JsonIgnore]
        public Ship Ship { get; private set; }

        [JsonProperty("soldshipid")]
        public int? soldshipid { get; private set; }

        [JsonProperty("soldship")]
        public string soldship { get; private set; }

        [JsonIgnore]
        public Ship SoldShip { get; private set; }

        [JsonProperty("storedshipid")]
        public int? storedshipid { get; private set; }

        [JsonProperty("storedship")]
        public string storedship { get; private set; }

        [JsonIgnore]
        public Ship StoredShip { get; private set; }

        public ShipSwappedEvent(DateTime timestamp, Ship ship, Ship soldship, Ship storedship) : base(timestamp, NAME)
        {
            this.Ship = ship;
            this.ship = (ship == null ? null : ship.model);
            this.shipid = (ship == null ? (int?)null : ship.LocalId);
            this.SoldShip = SoldShip;
            this.soldship = (soldship == null ? null : soldship.model);
            this.soldshipid = (soldship == null ? (int?)null : soldship.LocalId);
            this.StoredShip = StoredShip;
            this.storedship = (storedship == null ? null : storedship.model);
            this.storedshipid = (storedship == null ? (int?)null : storedship.LocalId);
        }
    }
}
