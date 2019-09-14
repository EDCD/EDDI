using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipDeliveredEvent : Event
    {
        public const string NAME = "Ship delivered";
        public const string DESCRIPTION = "Triggered when your newly-purchased ship is delivered to you";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-20T18:14:26Z\", \"event\":\"ShipyardBuy\", \"ShipType\":\"federation_corvette\", \"ShipPrice\":18796945, \"SellOldShip\":\"CobraMkIII\", \"SellShipID\":42, \"SellPrice\":950787 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipDeliveredEvent()
        {
            VARIABLES.Add("ship", "The ship that was delivered");
            VARIABLES.Add("shipid", "The ID of the ship that was delivered");
        }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("ship")]
        public string ship => shipDefinition?.model;

        // Not intended to be user facing
        public Ship shipDefinition { get; private set; }

        public ShipDeliveredEvent(DateTime timestamp, string ship, int? shipId) : base(timestamp, NAME)
        {
            this.shipDefinition = ShipDefinitions.FromEDModel(ship);
            this.shipid = shipId;
        }
    }
}
