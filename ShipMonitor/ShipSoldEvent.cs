using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class ShipSoldEvent : Event
    {
        public const string NAME = "Ship sold";
        public const string DESCRIPTION = "Triggered when you sell a ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ShipyardSell\",\"ShipType\":\"Adder\",\"SellShipID\":1,\"ShipPrice\":25000, \"MarketID\": 128666762}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipSoldEvent()
        {
            VARIABLES.Add("ship", "The ship that was sold");
            VARIABLES.Add("shipid", "The ID of the ship that was sold");
            VARIABLES.Add("price", "The price for which the ship was sold");
            VARIABLES.Add("system", "The system where the ship was sold");
        }

        public string ship => shipDefinition?.model;

        public int? shipid { get; private set; }

        public long price { get; private set; }

        public string system { get; private set; }

        // Not intended to be user facing
        [VoiceAttackIgnore]
        public Ship shipDefinition => ShipDefinitions.FromEDModel(edModel);

        [VoiceAttackIgnore]
        public long marketId { get; private set; }

        [VoiceAttackIgnore]
        public string edModel { get; private set; }

        public ShipSoldEvent(DateTime timestamp, string ship, int shipId, long price, string system, long marketId) : base(timestamp, NAME)
        {
            this.edModel = ship;
            this.shipid = shipId;
            this.price = price;
            this.system = system;
            this.marketId = marketId;
        }
    }
}
