﻿using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

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
            VARIABLES.Add("storedshipid", "The ID of the ship that was stored as part of the purchase");
        }

        [PublicAPI]
        public string ship => shipDefinition?.model;

        [PublicAPI]
        public long price { get; private set; }

        [PublicAPI]
        public string soldship => soldShipDefinition?.model;

        [PublicAPI]
        public int? soldshipid { get; private set; }

        [PublicAPI]
        public long? soldprice { get; private set; }

        [PublicAPI]
        public string storedship => storedShipDefinition?.model;

        [PublicAPI]
        public int? storedshipid { get; private set; }

        // Not intended to be user facing

        public Ship shipDefinition => ShipDefinitions.FromEDModel(edModel);
        
        public Ship storedShipDefinition => string.IsNullOrEmpty(storedEdModel) ? null : ShipDefinitions.FromEDModel(storedEdModel);

        public Ship soldShipDefinition => string.IsNullOrEmpty(soldEdModel) ? null : ShipDefinitions.FromEDModel(soldEdModel);

        public string edModel { get; private set; }

        public string storedEdModel { get; private set; }

        public string soldEdModel { get; private set; }

        public long marketId { get; private set; }

        public ShipPurchasedEvent(DateTime timestamp, string ship, long price, string soldShip, int? soldShipId, long? soldPrice, string storedShip, int? storedShipId, long marketId) : base(timestamp, NAME)
        {
            this.edModel = ship;
            this.storedEdModel = storedShip;
            this.soldEdModel = soldShip;
            this.price = price;
            this.soldshipid = soldShipId;
            this.soldprice = soldPrice;
            this.storedshipid = storedShipId;
            this.marketId = marketId;
        }
    }
}
