using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class ShipDeliveredEvent : Event
    {
        public const string NAME = "Ship delivered";
        public const string DESCRIPTION = "Triggered when your newly-purchased ship is delivered to you";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-20T18:14:26Z\", \"event\":\"ShipyardBuy\", \"ShipType\":\"federation_corvette\", \"ShipPrice\":18796945, \"SellOldShip\":\"CobraMkIII\", \"SellShipID\":42, \"SellPrice\":950787 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipDeliveredEvent()
        {
            VARIABLES.Add("shipid", "The ID of the ship that was delivered");
            VARIABLES.Add("ship", "The ship that was delivered");
        }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonIgnore]
        public Ship Ship { get; private set; }

        public ShipDeliveredEvent(DateTime timestamp, Ship ship) : base(timestamp, NAME)
        {
            this.Ship = ship;
            this.ship = (ship == null ? null : ship.model);
            this.shipid = (ship == null ? (int?)null : ship.LocalId);
        }
    }
}
